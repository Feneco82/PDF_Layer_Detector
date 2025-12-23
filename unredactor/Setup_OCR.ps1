# PDF Layer Detector - OCR Setup Script (PowerShell)
# ====================================================

Write-Host "PDF Layer Detector - OCR Setup Script" -ForegroundColor Yellow
Write-Host "====================================="
Write-Host ""
Write-Host "This script will download the English language data file for Tesseract OCR."
Write-Host ""

# Create tessdata directory if it doesn't exist
if (-not (Test-Path "tessdata")) {
    Write-Host "Creating tessdata directory..." -ForegroundColor Green
    New-Item -ItemType Directory -Path "tessdata" | Out-Null
}

# Check if eng.traineddata already exists
if (Test-Path "tessdata\eng.traineddata") {
    $fileInfo = Get-Item "tessdata\eng.traineddata"
    Write-Host "English language data already exists." -ForegroundColor Yellow
    Write-Host "File: tessdata\eng.traineddata"
    Write-Host "Size: $($fileInfo.Length) bytes"
    Write-Host "Modified: $($fileInfo.LastWriteTime)"
    Write-Host ""
    
    $overwrite = Read-Host "Do you want to download it again? (y/n)"
    if ($overwrite -ne "y" -and $overwrite -ne "Y") {
        Write-Host "Setup cancelled." -ForegroundColor Yellow
        Read-Host "Press Enter to exit"
        exit
    }
}

Write-Host "Downloading English language data..." -ForegroundColor Green
Write-Host "Source: https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata"
Write-Host ""

try {
    # Show progress
    $ProgressPreference = 'Continue'
    
    Invoke-WebRequest -Uri "https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata" `
                     -OutFile "tessdata\eng.traineddata" `
                     -UseBasicParsing
    
    Write-Host ""
    Write-Host "Download completed successfully!" -ForegroundColor Green
    
    # Verify download
    if (Test-Path "tessdata\eng.traineddata") {
        $fileInfo = Get-Item "tessdata\eng.traineddata"
        
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "OCR Setup completed successfully!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "File downloaded: tessdata\eng.traineddata"
        Write-Host "Size: $($fileInfo.Length) bytes"
        Write-Host "Modified: $($fileInfo.LastWriteTime)"
        Write-Host ""
        Write-Host "You can now use OCR functionality in the PDF Layer Detector." -ForegroundColor Cyan
        Write-Host "When prompted, choose 'y' to enable OCR for image-based PDFs." -ForegroundColor Cyan
    }
}
catch {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "Download failed!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "You can manually download the file from:" -ForegroundColor Yellow
    Write-Host "https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata"
    Write-Host ""
    Write-Host "Save it as: tessdata\eng.traineddata" -ForegroundColor Yellow
}

# =============================================
# PDF Layer Detector - OCR SETUP - NO LONGER NEEDED!
# =============================================

Write-Host "=============================================" -ForegroundColor Yellow
Write-Host "PDF Layer Detector - OCR SETUP - NO LONGER NEEDED!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "GOOD NEWS: OCR functionality is now BUILT INTO PDF Layer Detector!" -ForegroundColor Green
Write-Host ""
Write-Host "As of the latest version:" -ForegroundColor Cyan
Write-Host "  ? OCR is included by default" -ForegroundColor Green
Write-Host "  ? No setup required" -ForegroundColor Green
Write-Host "  ? English language data is bundled" -ForegroundColor Green
Write-Host "  ? Works immediately when you run the program" -ForegroundColor Green
Write-Host ""
Write-Host "=============================================" -ForegroundColor Yellow
Write-Host "What to do now:" -ForegroundColor Yellow
Write-Host "=============================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Just run ""PDF Layer Detector.exe"" directly" -ForegroundColor White
Write-Host "2. When prompted, choose 'y' to enable OCR" -ForegroundColor White
Write-Host "3. Enjoy complete PDF analysis with OCR!" -ForegroundColor White
Write-Host ""
Write-Host "This setup script is no longer needed." -ForegroundColor Yellow
Write-Host "You can safely delete Setup_OCR.bat and Setup_OCR.ps1" -ForegroundColor Yellow
Write-Host ""
Write-Host "=============================================" -ForegroundColor Yellow
Write-Host "Adding Additional Languages (Optional):" -ForegroundColor Yellow
Write-Host "=============================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "To add support for other languages:" -ForegroundColor Cyan
Write-Host "1. Download .traineddata files from:" -ForegroundColor White
Write-Host "   https://github.com/tesseract-ocr/tessdata" -ForegroundColor Blue
Write-Host "2. Place them in the tessdata folder next to ""PDF Layer Detector.exe""" -ForegroundColor White
Write-Host ""
Write-Host "Available languages: Portuguese, Spanish, French, German, etc." -ForegroundColor Cyan
Write-Host ""
Read-Host "Press Enter to exit"