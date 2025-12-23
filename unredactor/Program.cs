using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Tokens;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using SysImageFormat = System.Drawing.Imaging.ImageFormat;
using Tesseract;
using PdfiumViewer;

namespace PdfLayerDetector
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            
            Console.WriteLine("PDF Layer Detector - Using PdfPig (Apache 2.0 License)");
            Console.WriteLine("Clean, license-message-free PDF processing!");
            Console.WriteLine("Features: Layer Detection + Text Search + OCR in ALL PDFs");
            Console.WriteLine();
            
            string folderPath;
            
            // Get folder path from command line argument or user input
            if (args.Length > 0)
            {
                folderPath = args[0];
            }
            else
            {
                Console.Write("Enter the folder path to scan for PDF files: ");
                folderPath = Console.ReadLine() ?? string.Empty;
            }
            
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                Console.WriteLine("Invalid folder path. Please provide a valid directory.");
                return;
            }
            
            // Check OCR availability
            bool ocrAvailable = CheckOcrAvailability();
            
            // Ask for text search options
            Console.WriteLine("\nTEXT SEARCH OPTIONS:");
            Console.WriteLine("Note: Text search will be performed on ALL PDF files, regardless of layer status");
            Console.Write("Do you want to search for specific text in PDFs? (y/n): ");
            var enableTextSearch = Console.ReadLine()?.ToLower() == "y";
            
            string searchText = "";
            bool useRegex = false;
            bool enableOCR = false;
            
            if (enableTextSearch)
            {
                Console.Write("Enter text to search for (searches ALL PDFs): ");
                searchText = Console.ReadLine() ?? "";
                
                if (!string.IsNullOrEmpty(searchText))
                {
                    Console.Write("Use regex pattern matching? (y/n): ");
                    useRegex = Console.ReadLine()?.ToLower() == "y";
                }
                
                if (ocrAvailable)
                {
                    Console.Write("Enable OCR for image-based PDFs? (slower but comprehensive) (y/n): ");
                    enableOCR = Console.ReadLine()?.ToLower() == "y";
                    
                    if (enableOCR)
                    {
                        Console.WriteLine("OCR enabled - will process image-based PDFs");
                    }
                }
                else
                {
                    Console.WriteLine("OCR not available - will use direct text extraction only");
                    Console.WriteLine("To enable OCR: Download tessdata files to ./tessdata/ folder");
                }
            }
            
            Console.WriteLine($"\nScanning folder: {folderPath}");
            if (enableTextSearch && !string.IsNullOrEmpty(searchText))
            {
                Console.WriteLine($"Searching for: \"{searchText}\" in ALL PDF files");
                if (enableOCR) Console.WriteLine("OCR enabled for image-based PDFs");
            }
            Console.WriteLine(new string('-', 50));
            
            var pdfFiles = Directory.GetFiles(folderPath, "*.pdf", SearchOption.AllDirectories);
            
            if (pdfFiles.Length == 0)
            {
                Console.WriteLine("No PDF files found in the specified directory.");
                return;
            }
            
            Console.WriteLine($"Found {pdfFiles.Length} PDF file(s). Analyzing layers and text in ALL files...\n");
            
            var filesWithLayers = new List<PdfAnalysisResult>();
            var filesWithoutLayers = new List<PdfAnalysisResult>();
            var errorFiles = new List<(string file, string error)>();
            var startTime = DateTime.Now;
            
            foreach (var pdfFile in pdfFiles)
            {
                try
                {
                    var relativePath = Path.GetRelativePath(folderPath, pdfFile);
                    Console.Write($"Checking: {relativePath}... ");
                    
                    var result = AnalyzePdf(pdfFile, relativePath, searchText, useRegex, enableTextSearch, enableOCR);
                    
                    if (result.HasLayers)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("HAS LAYERS ?");
                        if (result.HasSearchText)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write(" + TEXT FOUND");
                        }
                        if (result.UsedOCR)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write(" (OCR)");
                        }
                        Console.WriteLine();
                        Console.ResetColor();
                        filesWithLayers.Add(result);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write("No layers");
                        if (result.HasSearchText)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write(" + TEXT FOUND");
                        }
                        if (result.UsedOCR)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write(" (OCR)");
                        }
                        Console.WriteLine();
                        Console.ResetColor();
                        filesWithoutLayers.Add(result);
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Console.ResetColor();
                    errorFiles.Add((Path.GetRelativePath(folderPath, pdfFile), ex.Message));
                }
            }
            
            var endTime = DateTime.Now;
            var processingTime = endTime - startTime;
            
            // Generate and display resume
            var resume = GenerateResume(folderPath, pdfFiles.Length, filesWithLayers, filesWithoutLayers, 
                errorFiles, startTime, endTime, processingTime, searchText, enableTextSearch, enableOCR, ocrAvailable);
            
            Console.WriteLine(resume);
            
            // Ask user if they want to save the resume to a file
            Console.Write("Do you want to save this resume to a file? (y/n): ");
            var saveResponse = Console.ReadLine()?.ToLower();
            
            if (saveResponse == "y" || saveResponse == "yes")
            {
                SaveResumeToFile(resume, folderPath);
            }
        }
        
        static bool CheckOcrAvailability()
        {
            try
            {
                // Check if tessdata directory exists
                var tessdataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
                if (!Directory.Exists(tessdataPath))
                {
                    return false;
                }
                
                // Check if English language data exists
                var engDataPath = Path.Combine(tessdataPath, "eng.traineddata");
                if (!File.Exists(engDataPath))
                {
                    return false;
                }
                
                // Try to initialize Tesseract engine
                using var engine = new TesseractEngine(tessdataPath, "eng", EngineMode.Default);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        static PdfAnalysisResult AnalyzePdf(string pdfFilePath, string relativePath, string searchText, 
            bool useRegex, bool enableTextSearch, bool enableOCR)
        {
            var result = new PdfAnalysisResult
            {
                FilePath = relativePath,
                HasLayers = HasLayers(pdfFilePath)
            };
            
            if (enableTextSearch)
            {
                // Extract text directly from PDF
                result.ExtractedText = ExtractTextFromPdf(pdfFilePath);
                
                // Try OCR if enabled and direct extraction yielded little text
                if (enableOCR && (string.IsNullOrWhiteSpace(result.ExtractedText) || result.ExtractedText.Trim().Length < 50))
                {
                    try
                    {
                        var ocrText = ExtractTextWithOCR(pdfFilePath);
                        if (!string.IsNullOrWhiteSpace(ocrText) && ocrText.Length > 10)
                        {
                            result.ExtractedText = string.IsNullOrWhiteSpace(result.ExtractedText) ? 
                                ocrText : result.ExtractedText + "\n\n[OCR Results]\n" + ocrText;
                            result.UsedOCR = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.OcrError = ex.Message;
                    }
                }
                
                // Check if we extracted meaningful text
                if (string.IsNullOrWhiteSpace(result.ExtractedText) || result.ExtractedText.Trim().Length < 10)
                {
                    result.ExtractedText = "[No extractable text found - may be image-based PDF]";
                }
                
                // Search for specific text if provided
                if (!string.IsNullOrEmpty(searchText) && !result.ExtractedText.StartsWith("[No extractable"))
                {
                    if (useRegex)
                    {
                        try
                        {
                            result.HasSearchText = Regex.IsMatch(result.ExtractedText, searchText, RegexOptions.IgnoreCase);
                            if (result.HasSearchText)
                            {
                                var matches = Regex.Matches(result.ExtractedText, searchText, RegexOptions.IgnoreCase);
                                result.TextMatches = matches.Cast<Match>().Select(m => m.Value).Take(5).ToList();
                            }
                        }
                        catch (Exception ex)
                        {
                            result.SearchError = $"Regex error: {ex.Message}";
                        }
                    }
                    else
                    {
                        result.HasSearchText = result.ExtractedText.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                        if (result.HasSearchText)
                        {
                            // Find context around matches
                            result.TextMatches = FindTextMatches(result.ExtractedText, searchText);
                        }
                    }
                }
            }
            
            return result;
        }
        
        static List<string> FindTextMatches(string text, string searchText)
        {
            var matches = new List<string>();
            var index = 0;
            
            while ((index = text.IndexOf(searchText, index, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                var start = Math.Max(0, index - 30);
                var end = Math.Min(text.Length, index + searchText.Length + 30);
                var context = text.Substring(start, end - start).Replace('\n', ' ').Replace('\r', ' ');
                matches.Add($"...{context}...");
                
                index += searchText.Length;
                if (matches.Count >= 5) break; // Limit to 5 matches
            }
            
            return matches;
        }
        
        static string ExtractTextWithOCR(string pdfFilePath)
        {
            try
            {
                var tessdataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
                using var engine = new TesseractEngine(tessdataPath, "eng", EngineMode.Default);
                
                var allText = new StringBuilder();
                
                try
                {
                    // Use PdfiumViewer for PDF to image conversion
                    using var pdfViewer = PdfiumViewer.PdfDocument.Load(pdfFilePath);
                    
                    for (int pageIndex = 0; pageIndex < pdfViewer.PageCount; pageIndex++)
                    {
                        // Render page to image
                        using var image = pdfViewer.Render(pageIndex, 150, 150, false); // 150 DPI
                        
                        // Convert to format suitable for Tesseract
                        using var bitmap = new Bitmap(image);
                        
                        // Convert bitmap to byte array for Tesseract
                        using var memoryStream = new MemoryStream();
                        bitmap.Save(memoryStream, SysImageFormat.Png);
                        var imageBytes = memoryStream.ToArray();
                        
                        // OCR the image
                        using var pix = Tesseract.Pix.LoadFromMemory(imageBytes);
                        using var page = engine.Process(pix);
                        var pageText = page.GetText();
                        
                        if (!string.IsNullOrWhiteSpace(pageText))
                        {
                            allText.AppendLine($"[Page {pageIndex + 1}]");
                            allText.AppendLine(pageText.Trim());
                            allText.AppendLine();
                        }
                    }
                }
                catch
                {
                    // Fallback: Try alternative method or return placeholder
                    return "OCR processing attempted but failed for this PDF";
                }
                
                var result = allText.ToString().Trim();
                return string.IsNullOrWhiteSpace(result) ? "No text found via OCR" : result;
            }
            catch (Exception ex)
            {
                throw new Exception($"OCR processing failed: {ex.Message}");
            }
        }
        
        static string ExtractTextFromPdf(string pdfFilePath)
        {
            try
            {
                using var document = UglyToad.PdfPig.PdfDocument.Open(pdfFilePath);
                var textBuilder = new StringBuilder();
                
                foreach (var page in document.GetPages())
                {
                    var pageText = page.Text;
                    if (!string.IsNullOrWhiteSpace(pageText))
                    {
                        textBuilder.AppendLine(pageText);
                    }
                }
                
                return textBuilder.ToString().Trim();
            }
            catch (Exception ex)
            {
                return $"[Error extracting text: {ex.Message}]";
            }
        }
        
        static bool HasLayers(string pdfFilePath)
        {
            try
            {
                using var document = UglyToad.PdfPig.PdfDocument.Open(pdfFilePath);
                
                // Method 1: Check document catalog for OCProperties
                try
                {
                    var catalogDict = document.Structure.Catalog.CatalogDictionary;
                    
                    if (catalogDict.Data.ContainsKey("OCProperties"))
                    {
                        var ocPropertiesToken = catalogDict.Data["OCProperties"];
                        if (ocPropertiesToken is DictionaryToken ocProperties)
                        {
                            // Check for OCGs (Optional Content Groups)
                            if (ocProperties.Data.ContainsKey("OCGs"))
                            {
                                var ocgsToken = ocProperties.Data["OCGs"];
                                if (ocgsToken is ArrayToken ocgsArray && ocgsArray.Data.Count > 0)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Continue with alternative methods if catalog parsing fails
                }
                
                // Method 2: Check pages for layer references
                try
                {
                    foreach (var page in document.GetPages())
                    {
                        var pageDict = page.Dictionary;
                        
                        // Check for Resources dictionary
                        if (pageDict.Data.ContainsKey("Resources"))
                        {
                            var resourcesToken = pageDict.Data["Resources"];
                            if (resourcesToken is DictionaryToken resourcesDict)
                            {
                                // Check for Properties in resources (layer references)
                                if (resourcesDict.Data.ContainsKey("Properties"))
                                {
                                    var propertiesToken = resourcesDict.Data["Properties"];
                                    if (propertiesToken is DictionaryToken properties && properties.Data.Count > 0)
                                    {
                                        return true;
                                    }
                                }
                                
                                // Check ExtGState for optional content references
                                if (resourcesDict.Data.ContainsKey("ExtGState"))
                                {
                                    var extGStateToken = resourcesDict.Data["ExtGState"];
                                    if (extGStateToken is DictionaryToken extGStates)
                                    {
                                        foreach (var stateEntry in extGStates.Data)
                                        {
                                            if (stateEntry.Value is DictionaryToken stateDict && stateDict.Data.ContainsKey("OC"))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Continue if page parsing fails
                }
                
                // Method 3: Simple text-based detection as fallback
                try
                {
                    using var fileStream = File.OpenRead(pdfFilePath);
                    using var reader = new StreamReader(fileStream);
                    var content = reader.ReadToEnd();
                    
                    // Look for common layer-related keywords in the PDF content
                    if (content.Contains("/OCProperties") || 
                        content.Contains("/OCGs") || 
                        content.Contains("/OC ") ||
                        (content.Contains("BDC") && content.Contains("OC")))
                    {
                        return true;
                    }
                }
                catch
                {
                    // If all methods fail, assume no layers
                }
                
                return false;
            }
            catch
            {
                // If PDF can't be opened, assume no layers
                return false;
            }
        }
        
        static string GenerateResume(string folderPath, int totalFiles, List<PdfAnalysisResult> filesWithLayers, 
            List<PdfAnalysisResult> filesWithoutLayers, List<(string file, string error)> errorFiles, 
            DateTime startTime, DateTime endTime, TimeSpan processingTime, string searchText, bool enableTextSearch, bool enableOCR, bool ocrAvailable)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine(new string('=', 80));
            sb.AppendLine("PDF LAYER DETECTION & TEXT ANALYSIS RESUME");
            sb.AppendLine(new string('=', 80));
            sb.AppendLine();
            
            // Scan Information
            sb.AppendLine("SCAN INFORMATION:");
            sb.AppendLine($"  Scanned Folder: {folderPath}");
            sb.AppendLine($"  Scan Date: {startTime:yyyy-MM-dd}");
            sb.AppendLine($"  Scan Time: {startTime:HH:mm:ss} - {endTime:HH:mm:ss}");
            sb.AppendLine($"  Processing Duration: {processingTime:mm\\:ss\\.fff}");
            sb.AppendLine($"  Total Files Found: {totalFiles}");
            sb.AppendLine($"  PDF Library: PdfPig (Apache 2.0 License)");
            sb.AppendLine($"  Text Search Enabled: {(enableTextSearch ? "Yes" : "No")}");
            sb.AppendLine($"  OCR Available: {(ocrAvailable ? "Yes" : "No")}");
            sb.AppendLine($"  OCR Enabled: {(enableOCR ? "Yes" : "No")}");
            if (enableTextSearch && !string.IsNullOrEmpty(searchText))
            {
                sb.AppendLine($"  Search Term: \"{searchText}\"");
                sb.AppendLine($"  Search Scope: ALL PDF files (with and without layers)");
            }
            sb.AppendLine();
            
            // Statistics
            var totalProcessed = filesWithLayers.Count + filesWithoutLayers.Count;
            var allFiles = filesWithLayers.Concat(filesWithoutLayers).ToList();
            var filesWithText = allFiles.Count(f => f.HasSearchText);
            var filesWithExtractableText = allFiles
                .Count(f => !string.IsNullOrEmpty(f.ExtractedText) && !f.ExtractedText.StartsWith("[No extractable") && !f.ExtractedText.StartsWith("[Error"));
            var filesProcessedWithOCR = allFiles.Count(f => f.UsedOCR);
            
            sb.AppendLine("STATISTICS:");
            sb.AppendLine($"  Files WITH layers: {filesWithLayers.Count} ({GetPercentage(filesWithLayers.Count, totalFiles):F1}%)");
            sb.AppendLine($"  Files WITHOUT layers: {filesWithoutLayers.Count} ({GetPercentage(filesWithoutLayers.Count, totalFiles):F1}%)");
            sb.AppendLine($"  Files with errors: {errorFiles.Count} ({GetPercentage(errorFiles.Count, totalFiles):F1}%)");
            
            if (enableTextSearch)
            {
                sb.AppendLine($"  Files with extractable text: {filesWithExtractableText} ({GetPercentage(filesWithExtractableText, totalProcessed):F1}%)");
                if (enableOCR)
                {
                    sb.AppendLine($"  Files processed with OCR: {filesProcessedWithOCR} ({GetPercentage(filesProcessedWithOCR, totalProcessed):F1}%)");
                }
                if (!string.IsNullOrEmpty(searchText))
                {
                    sb.AppendLine($"  Files with search matches (ALL): {filesWithText} ({GetPercentage(filesWithText, totalProcessed):F1}%)");
                    
                    // Break down by layer status
                    var layeredFilesWithText = filesWithLayers.Count(f => f.HasSearchText);
                    var nonLayeredFilesWithText = filesWithoutLayers.Count(f => f.HasSearchText);
                    sb.AppendLine($"    - In files WITH layers: {layeredFilesWithText}");
                    sb.AppendLine($"    - In files WITHOUT layers: {nonLayeredFilesWithText}");
                    
                    if (enableOCR)
                    {
                        var ocrMatches = allFiles.Count(f => f.HasSearchText && f.UsedOCR);
                        sb.AppendLine($"    - Found via OCR: {ocrMatches}");
                    }
                }
            }
            sb.AppendLine();
            
            // Show ALL files with text matches first (most important for text search)
            if (enableTextSearch && !string.IsNullOrEmpty(searchText) && filesWithText > 0)
            {
                sb.AppendLine($"ALL FILES WITH TEXT MATCHES ({filesWithText}):");
                sb.AppendLine(new string('=', 50));
                
                var allFilesWithText = allFiles.Where(f => f.HasSearchText).OrderBy(f => f.FilePath).ToList();
                for (int i = 0; i < allFilesWithText.Count; i++)
                {
                    var file = allFilesWithText[i];
                    sb.AppendLine($"  {i + 1,3}. {file.FilePath}");
                    
                    // Show layer status
                    if (file.HasLayers)
                    {
                        sb.AppendLine($"       ?? HAS LAYERS - May contain hidden content!");
                    }
                    else
                    {
                        sb.AppendLine($"       ?? Regular PDF (no layers)");
                    }
                    
                    // Show OCR usage
                    if (file.UsedOCR)
                    {
                        sb.AppendLine($"       ?? Text found via OCR (was image-based)");
                    }
                    
                    // Show matches
                    if (file.TextMatches?.Count > 0)
                    {
                        sb.AppendLine($"       Matches: {string.Join(" | ", file.TextMatches.Take(2))}");
                    }
                    
                    if (!string.IsNullOrEmpty(file.SearchError))
                    {
                        sb.AppendLine($"       ? Search error: {file.SearchError}");
                    }
                    
                    if (!string.IsNullOrEmpty(file.OcrError))
                    {
                        sb.AppendLine($"       ? OCR error: {file.OcrError}");
                    }
                }
                sb.AppendLine();
            }
            
            // Files with layers (detailed list) - simplified since we show matches above
            if (filesWithLayers.Count > 0)
            {
                sb.AppendLine($"FILES WITH LAYERS - SUMMARY ({filesWithLayers.Count}):");
                sb.AppendLine(new string('-', 50));
                foreach (var file in filesWithLayers)
                {
                    var index = filesWithLayers.IndexOf(file);
                    sb.Append($"  {index + 1,3}. {file.FilePath}");
                    if (file.HasSearchText) sb.Append(" [TEXT MATCH]");
                    if (file.UsedOCR) sb.Append(" [OCR]");
                    sb.AppendLine();
                }
                sb.AppendLine();
            }
            
            // OCR Summary
            if (enableOCR && filesProcessedWithOCR > 0)
            {
                sb.AppendLine($"OCR PROCESSING SUMMARY ({filesProcessedWithOCR} files):");
                sb.AppendLine(new string('-', 50));
                var ocrFiles = allFiles.Where(f => f.UsedOCR).ToList();
                foreach (var file in ocrFiles)
                {
                    var index = ocrFiles.IndexOf(file);
                    sb.AppendLine($"  {index + 1,3}. {file.FilePath}");
                    if (file.HasSearchText)
                    {
                        sb.AppendLine($"       ? Text matches found via OCR");
                    }
                    if (!string.IsNullOrEmpty(file.OcrError))
                    {
                        sb.AppendLine($"       ? OCR error: {file.OcrError}");
                    }
                }
                sb.AppendLine();
            }
            
            // Error files
            if (errorFiles.Count > 0)
            {
                sb.AppendLine($"FILES WITH ERRORS ({errorFiles.Count}):");
                sb.AppendLine(new string('-', 40));
                for (int i = 0; i < errorFiles.Count; i++)
                {
                    sb.AppendLine($"  {i + 1,3}. {errorFiles[i].file}");
                    sb.AppendLine($"       Error: {errorFiles[i].error}");
                }
                sb.AppendLine();
            }
            
            // Summary
            sb.AppendLine("SUMMARY:");
            if (filesWithLayers.Count > 0)
            {
                sb.AppendLine($"  ?? Found {filesWithLayers.Count} PDF file(s) containing layers");
                sb.AppendLine("  ? These files may contain redacted content that can be unhidden");
            }
            else
            {
                sb.AppendLine("  • No PDF files with layers were found");
            }
            
            if (enableTextSearch)
            {
                if (filesWithText > 0 && !string.IsNullOrEmpty(searchText))
                {
                    sb.AppendLine($"  ?? Found search text in {filesWithText} file(s) across ALL PDFs");
                    var layeredMatches = filesWithLayers.Count(f => f.HasSearchText);
                    if (layeredMatches > 0)
                    {
                        sb.AppendLine($"  ?  {layeredMatches} of these matches are in layered PDFs (potential security risk)");
                    }
                    
                    if (enableOCR)
                    {
                        var ocrMatches = allFiles.Count(f => f.HasSearchText && f.UsedOCR);
                        if (ocrMatches > 0)
                        {
                            sb.AppendLine($"  ?? {ocrMatches} matches found only through OCR (were image-based)");
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(searchText))
                {
                    sb.AppendLine($"  • No matches found for \"{searchText}\" in any PDF files");
                }
                
                if (enableOCR)
                {
                    sb.AppendLine($"  ?? OCR processed {filesProcessedWithOCR} image-based PDF(s)");
                }
                else if (!ocrAvailable)
                {
                    var imageBasedFiles = allFiles.Count(f => f.ExtractedText?.StartsWith("[No extractable") == true);
                    if (imageBasedFiles > 0)
                    {
                        sb.AppendLine($"  ?? Found {imageBasedFiles} image-based PDF(s) - OCR not available");
                        sb.AppendLine($"  ? To enable OCR: Create ./tessdata/ folder with eng.traineddata");
                    }
                }
            }
            
            if (errorFiles.Count > 0)
            {
                sb.AppendLine($"  ? {errorFiles.Count} file(s) could not be processed");
            }
            
            sb.AppendLine();
            sb.AppendLine($"Scan completed at {endTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Text search performed on ALL {totalProcessed} PDF files");
            if (enableOCR) sb.AppendLine($"OCR processing enabled for image-based PDFs");
            sb.AppendLine(new string('=', 80));
            
            return sb.ToString();
        }
        
        static double GetPercentage(int count, int total)
        {
            return total > 0 ? (double)count / total * 100 : 0;
        }
        
        static void SaveResumeToFile(string resume, string originalFolderPath)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var folderName = Path.GetFileName(originalFolderPath.TrimEnd(Path.DirectorySeparatorChar));
                var safeFileName = $"PDF_Analysis_{folderName}_{timestamp}.txt";
                
                // Save in the same directory as the executable
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), safeFileName);
                
                File.WriteAllText(filePath, resume, Encoding.UTF8);
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Resume saved to: {filePath}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error saving resume: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
    
    public class PdfAnalysisResult
    {
        public string FilePath { get; set; } = string.Empty;
        public bool HasLayers { get; set; }
        public bool HasSearchText { get; set; }
        public string ExtractedText { get; set; } = string.Empty;
        public List<string> TextMatches { get; set; } = new List<string>();
        public bool UsedOCR { get; set; }
        public string? OcrError { get; set; }
        public string? SearchError { get; set; }
    }
}