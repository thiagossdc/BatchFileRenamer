using System;
using System.IO;
using System.Linq;

class BatchFileRenamer
{
    static void Main()
    {
        Console.WriteLine("Digite o caminho da pasta com os arquivos:");
        string folderPath = Console.ReadLine();

        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("Caminho inválido. Saindo...");
            return;
        }

        Console.WriteLine("Digite o prefixo desejado (ou pressione Enter para ignorar):");
        string prefix = Console.ReadLine();

        Console.WriteLine("Digite o sufixo desejado (ou pressione Enter para ignorar):");
        string suffix = Console.ReadLine();

        Console.WriteLine("Deseja adicionar numeração sequencial? (s/n):");
        bool addNumbering = Console.ReadLine()?.Trim().ToLower() == "s";

        var files = Directory.GetFiles(folderPath);
        int counter = 1;

        foreach (var file in files)
        {
            string directory = Path.GetDirectoryName(file);
            string extension = Path.GetExtension(file);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);

            string newFileName = $"{prefix}{fileNameWithoutExt}{suffix}{(addNumbering ? counter.ToString("D3") : "")}{extension}";
            string newFilePath = Path.Combine(directory, newFileName);

            try
            {
                File.Move(file, newFilePath);
                Console.WriteLine($"Renomeado: {file} -> {newFileName}");
                counter++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao renomear {file}: {ex.Message}");
            }
        }

        Console.WriteLine("Processo concluído!");
    }
}
