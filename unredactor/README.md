# PDF Layer Detector & Text Analyzer with OCR

This program scans PDF files in a folder (and its subfolders) to identify which files contain layers (Optional Content Groups/OCGs) and searches for text content **in ALL PDF files**, including image-based PDFs through OCR (Optical Character Recognition).

## Features

### Core Functionality
- **Recursive scanning**: Searches through all subfolders
- **Layer detection**: Identifies PDFs with Optional Content Groups (layers)
- **Universal text extraction**: Extracts searchable text from **ALL PDF files**
- **Built-in OCR processing**: Handles image-based and scanned PDFs via Tesseract OCR (**included by default**)
- **Comprehensive text search**: Search for specific text patterns **across ALL PDFs**
- **Regex support**: Advanced pattern matching with regular expressions

### Advanced Features
- **Color-coded output**: 
  - Green ? for files WITH layers
  - Yellow + for files with matching text (from ANY file)
  - Cyan (OCR) for files processed with OCR
  - Gray for files without layers or matches
  - Red ? for files with errors
- **Comprehensive resume**: Detailed summary report with statistics and timing
- **Complete text analysis**: Shows matches from layered, non-layered, and OCR-processed files
- **OCR statistics**: Detailed reporting on OCR usage and success
- **File export**: Option to save the resume to a timestamped text file
- **Performance tracking**: Shows scan duration and processing statistics
- **Error handling**: Gracefully handles corrupted or protected PDFs
- **Clean licensing**: No annoying license messages or restrictions

## Usage

### Option 1: Command Line Argument
```bash
"PDF Layer Detector.exe" "C:\path\to\your\pdf\folder"
```

### Option 2: Interactive Mode
Simply run the program and enter the folder path when prompted:
```bash
"PDF Layer Detector.exe"
```

## OCR Ready Out-of-the-Box

**OCR functionality is included by default!** No additional setup required.

- ? **Tesseract OCR engine** (included via NuGet)
- ? **English language data** (eng.traineddata) included in the program
- ? **Automatic availability** - OCR works immediately after installation
- ? **No external dependencies** - everything needed is bundled

### Additional Languages (Optional)

The program includes English by default. To add support for other languages:

1. Download additional `.traineddata` files from [Tesseract GitHub](https://github.com/tesseract-ocr/tessdata)
2. Place them in the `tessdata` folder next to your program
3. Available languages include:
   - **Portuguese**: `por.traineddata`
   - **Spanish**: `spa.traineddata`
   - **French**: `fra.traineddata`
   - **German**: `deu.traineddata`
   - **Italian**: `ita.traineddata`

## Interactive Options

When you run the program, you'll be prompted with several options:

1. **Text Search**: Choose whether to enable text extraction and searching
2. **Search Term**: Enter specific text to search for (searches **ALL PDFs**)
3. **Regex Patterns**: Use regular expressions for advanced pattern matching
4. **OCR Processing**: Enable OCR for image-based PDFs (**always available**)

**Important**: Text search is performed on **ALL PDF files** in the directory, including image-based PDFs when OCR is enabled!

## Sample Output with OCR

```
PDF Layer Detector - Using PdfPig (Apache 2.0 License)
Clean, license-message-free PDF processing!
Features: Layer Detection + Text Search + OCR in ALL PDFs

TEXT SEARCH OPTIONS:
Note: Text search will be performed on ALL PDF files, regardless of layer status
Do you want to search for specific text in PDFs? (y/n): y
Enter text to search for (searches ALL PDFs): confidential
Use regex pattern matching? (y/n): n
Enable OCR for image-based PDFs? (slower but comprehensive) (y/n): y
OCR enabled - will process image-based PDFs

Scanning folder: C:\MyPDFs
Searching for: "confidential" in ALL PDF files
OCR enabled for image-based PDFs
--------------------------------------------------
Found 6 PDF file(s). Analyzing layers and text in ALL files...

Checking: document1.pdf... No layers + TEXT FOUND
Checking: subfolder\layered_document.pdf... HAS LAYERS ? + TEXT FOUND
Checking: another_doc.pdf... No layers + TEXT FOUND
Checking: technical\blueprint.pdf... HAS LAYERS ?
Checking: scanned_invoice.pdf... No layers + TEXT FOUND (OCR)
Checking: image_based.pdf... No layers (OCR)

================================================================================
PDF LAYER DETECTION & TEXT ANALYSIS RESUME
================================================================================

SCAN INFORMATION:
  Scanned Folder: C:\MyPDFs
  Scan Date: 2024-01-15
  Scan Time: 14:30:15 - 14:30:45
  Processing Duration: 00:30.567
  Total Files Found: 6
  PDF Library: PdfPig (Apache 2.0 License)
  Text Search Enabled: Yes
  OCR Available: Yes
  OCR Enabled: Yes
  Search Term: "confidential"
  Search Scope: ALL PDF files (with and without layers)

STATISTICS:
  Files WITH layers: 2 (33.3%)
  Files WITHOUT layers: 4 (66.7%)
  Files with errors: 0 (0.0%)
  Files with extractable text: 5 (83.3%)
  Files processed with OCR: 2 (33.3%)
  Files with search matches (ALL): 4 (66.7%)
    - In files WITH layers: 1
    - In files WITHOUT layers: 3
    - Found via OCR: 1

ALL FILES WITH TEXT MATCHES (4):
==================================================
    1. another_doc.pdf
       ?? Regular PDF (no layers)
       Matches: ...this confidential report contains...
    2. document1.pdf
       ?? Regular PDF (no layers)
       Matches: ...confidential information stored...
    3. scanned_invoice.pdf
       ?? Regular PDF (no layers)
       ?? Text found via OCR (was image-based)
       Matches: ...confidential client data...
    4. subfolder\layered_document.pdf
       ?? HAS LAYERS - May contain hidden content!
       Matches: ...marked as confidential and should...

OCR PROCESSING SUMMARY (2 files):
--------------------------------------------------
    1. image_based.pdf
    2. scanned_invoice.pdf
       ? Text matches found via OCR

SUMMARY:
  ?? Found 2 PDF file(s) containing layers
  ? These files may contain redacted content that can be unhidden
  ?? Found search text in 4 file(s) across ALL PDFs
  ?  1 of these matches are in layered PDFs (potential security risk)
  ?? 1 matches found only through OCR (were image-based)
  ?? OCR processed 2 image-based PDF(s)

Scan completed at 2024-01-15 14:30:45
Text search performed on ALL 6 PDF files
OCR processing enabled for image-based PDFs
================================================================================
```

## Text Search Capabilities

### Universal Coverage
- **Searches ALL PDFs**: Direct text extraction + OCR for image-based files
- **Layer-aware results**: Results clearly indicate which matches come from layered PDFs
- **OCR integration**: Seamlessly processes scanned documents and images
- **Complete analysis**: No PDF is skipped during text analysis

### Direct Text Extraction
- Extracts searchable text from PDF documents
- Works with standard PDFs that contain embedded text
- Fast and efficient processing

### OCR Processing
- **Included by default**: No setup required, works out-of-the-box
- **Automatic detection**: Identifies image-based PDFs that need OCR
- **High-quality OCR**: Uses Tesseract engine for accurate text recognition
- **Multi-page support**: Processes all pages in image-based PDFs
- **Performance optimized**: OCR only triggered when direct extraction fails
- **Clear reporting**: OCR results are clearly marked in output

### Search Features
- **Simple text search**: Case-insensitive substring matching across ALL files
- **Regex patterns**: Advanced pattern matching with regular expressions
- **Context extraction**: Shows surrounding text for each match
- **Match limiting**: Displays up to 5 matches per file for readability
- **Complete coverage**: Every PDF (text-based and image-based) is analyzed

## Requirements

- .NET 10.0 or later
- PdfPig library (included via NuGet) - Apache 2.0 License
- **OCR included**: Tesseract engine and English language data bundled

## Use Cases

### Security Analysis with Complete Coverage
- **Comprehensive content discovery**: Search for sensitive terms across ALL documents
- **Scanned document analysis**: Find sensitive content in image-based PDFs
- **Redaction verification**: Identify layered PDFs AND find if content exists in scanned copies
- **Complete compliance auditing**: Ensure no sensitive content exists in ANY PDF file

### Document Management
- **Universal content indexing**: Extract and search text from both digital and scanned PDFs
- **Archive digitization**: Process legacy scanned documents alongside modern PDFs
- **Quality assurance**: Complete picture of text vs image-based documents
- **Data migration**: Extract content from old scanned archives

### Forensic Analysis
- **Evidence processing**: Extract text from all types of PDF evidence
- **Document reconstruction**: Find content across multiple document formats
- **Comprehensive discovery**: Ensure no document type is missed during analysis

## What are PDF Layers?

PDF layers (officially called Optional Content Groups or OCGs) allow content to be selectively viewed or hidden. They're commonly used in:
- Technical drawings and CAD files
- Maps with toggleable features
- Documents with multiple language versions
- Forms with conditional content
- **Redacted documents** where sensitive content may be hidden but not actually removed

## Security Note

**The program searches ALL files** which is especially important with OCR because:
- Sensitive content might exist in scanned copies of documents
- Redacted information might have been scanned and saved as images
- Complete security audits require checking every file type, including image-based PDFs
- OCR can reveal content in documents that appear to contain no searchable text

Files with layers should be carefully reviewed as they may contain:
- Hidden or redacted content that can be made visible
- Multiple versions of the same information
- Sensitive data that appears to be removed but is actually just hidden

## Performance Considerations

### OCR Processing
- **Included by default**: No setup overhead or complexity
- **Slower than direct text extraction**: OCR requires image processing
- **Automatic optimization**: Only triggered for image-based PDFs
- **Configurable**: Can be disabled for faster processing
- **Progress indication**: Clear feedback on processing status

### Recommendations
- **Enable OCR when needed**: Use for comprehensive analysis
- **OCR is optional**: Can be disabled during runtime for faster processing
- **Batch processing**: Process large sets in smaller groups
- **Targeted searches**: Use specific terms to reduce processing time

## Advantages of Bundled OCR

**Why OCR is included by default:**
- ? **Zero setup required**: Works immediately after installation
- ? **No external dependencies**: Everything needed is included
- ? **Always available**: OCR option always present in the interface
- ? **Consistent experience**: Same functionality on all machines
- ? **Professional deployment**: No setup steps for end users

## Troubleshooting

### Cannot Process Certain PDFs
- Some PDFs may be corrupted, password-protected, or use unsupported features
- OCR requires readable images - poor quality scans may not process well
- These will be reported in the error section of the resume

### Performance Issues
- OCR processing is inherently slower than direct text extraction
- Consider disabling OCR for quick scans when you know files are text-based
- Use specific search terms to reduce processing time
- Process smaller batches for very large document sets

### Text Search Behavior
- **All files are searched**: The program never skips PDF files during text analysis
- **OCR as fallback**: Used only when direct extraction yields minimal text
- **Complete coverage**: Every accessible PDF in the directory tree is analyzed
- **Always available**: OCR functionality is always ready to use