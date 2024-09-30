using NAudio.Lame;
using NAudio.Wave;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Write("Digite o link do vídeo que deseja baixar: ");
        string link = Console.ReadLine();

        Console.Write("Digite o diretório ou aperte [ENTER] para continuar com o padrão (C:\\YTMusicD): ");
        string path = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(path))
        {
            path = @"C:\YTMusicD";

            if (!Directory.Exists(path))
            {
                Console.WriteLine($"O diretório {path} não existe, criando...");
                Directory.CreateDirectory(path);
            }
            else
                Console.WriteLine($"O diretório {path} já existe.");
        }

        await DownloadAndConvert(link, path);
    }

    static async Task DownloadAndConvert(string link, string path)
    {
        try
        {
            var youtube = new YoutubeClient();

            var video = await youtube.Videos.GetAsync(link);
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);

            var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            if (audioStreamInfo == null)
            {
                Console.WriteLine("Erro ao obter o stream de áudio.");
                return;
            }

            string sanitizedTitle = SanitizeFileName(video.Title);

            string audioFilePath = Path.Combine(path, $"{sanitizedTitle}.mp4");

            Console.WriteLine("Baixando áudio...");
            await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, audioFilePath);

            Console.WriteLine("Download de áudio completo!");

            Console.WriteLine("Convertendo para MP3...");
            string mp3FilePath = Path.Combine(path, Path.GetFileNameWithoutExtension(audioFilePath) + ".mp3");

            using (var reader = new AudioFileReader(audioFilePath))
            {
                using (var writer = new LameMP3FileWriter(mp3FilePath, reader.WaveFormat, LAMEPreset.VBR_90))
                {
                    reader.CopyTo(writer);
                }
            }

            File.Delete(audioFilePath);
            Console.WriteLine("Conversão e limpeza completadas com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
    }

    static string SanitizeFileName(string fileName) =>
        Regex.Replace(fileName, @"[\/:*?""<>|]", string.Empty);
}