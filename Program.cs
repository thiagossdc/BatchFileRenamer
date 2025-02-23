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

        Console.WriteLine("Digite a extensão dos arquivos a renomear (ex: .txt, .jpg, ou pressione Enter para todas):");
        string extensionFilter = Console.ReadLine()?.Trim();

        Console.WriteLine("Digite o prefixo desejado (ou pressione Enter para ignorar):");
        string prefix = Console.ReadLine();

        Console.WriteLine("Digite o sufixo desejado (ou pressione Enter para ignorar):");
        string suffix = Console.ReadLine();

        Console.WriteLine("Deseja adicionar numeração sequencial? (s/n):");
        bool addNumbering = Console.ReadLine()?.Trim().ToLower() == "s";

        Console.WriteLine("Deseja visualizar antes de renomear? (s/n):");
        bool preview = Console.ReadLine()?.Trim().ToLower() == "s";

        var files = Directory.GetFiles(folderPath)
            .Where(file => string.IsNullOrEmpty(extensionFilter) || Path.GetExtension(file).Equals(extensionFilter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (files.Count == 0)
        {
            Console.WriteLine("Nenhum arquivo encontrado com os critérios fornecidos.");
            return;
        }

        List<(string oldPath, string newPath)> renameHistory = new List<(string, string)>();
        int counter = 1;

        if (preview)
        {
            Console.WriteLine("\nPré-visualização das mudanças:");
            foreach (var file in files)
            {
                string newFileName = GenerateNewFileName(file, prefix, suffix, addNumbering ? counter.ToString("D3") : "");
                Console.WriteLine($"{Path.GetFileName(file)} -> {newFileName}");
                counter++;
            }

            Console.WriteLine("\nConfirmar renomeação? (s/n):");
            if (Console.ReadLine()?.Trim().ToLower() != "s")
            {
                Console.WriteLine("Operação cancelada.");
                return;
            }
        }

        counter = 1;

        Parallel.ForEach(files, file =>
        {
            string newFileName = GenerateNewFileName(file, prefix, suffix, addNumbering ? counter.ToString("D3") : "");
            string newFilePath = Path.Combine(Path.GetDirectoryName(file), newFileName);

            try
            {
                File.Move(file, newFilePath);
                lock (renameHistory)
                {
                    renameHistory.Add((file, newFilePath));
                    Console.WriteLine($"Renomeado: {Path.GetFileName(file)} -> {newFileName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao renomear {file}: {ex.Message}");
            }
        });

        string logFilePath = Path.Combine(folderPath, "rename_log.txt");
        File.WriteAllLines(logFilePath, renameHistory.Select(entry => $"{entry.oldPath} -> {entry.newPath}"));
        Console.WriteLine($"Log salvo em: {logFilePath}");

        Console.WriteLine("Processo concluído!");

        Console.WriteLine("Deseja desfazer a última renomeação? (s/n):");
        if (Console.ReadLine()?.Trim().ToLower() == "s")
        {
            UndoLastRename(renameHistory);
        }
    }

    static string GenerateNewFileName(string file, string prefix, string suffix, string numbering)
    {
        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
        string extension = Path.GetExtension(file);
        return $"{prefix}{fileNameWithoutExt}{suffix}{numbering}{extension}";
    }

    static void UndoLastRename(List<(string oldPath, string newPath)> renameHistory)
    {
        foreach (var (oldPath, newPath) in renameHistory)
        {
            try
            {
                File.Move(newPath, oldPath);
                Console.WriteLine($"Revertido: {Path.GetFileName(newPath)} -> {Path.GetFileName(oldPath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao desfazer renomeação {newPath}: {ex.Message}");
            }
        }
        Console.WriteLine("Renomeação desfeita com sucesso!");
    }
}
