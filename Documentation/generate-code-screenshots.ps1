Add-Type -AssemblyName System.Drawing

function New-CodeScreenshot {
    param(
        [string]$InputPath,
        [int]$Skip,
        [int]$First,
        [string]$OutputPath,
        [string]$Title
    )

    $lines = Get-Content $InputPath | Select-Object -Skip $Skip -First $First
    $numbered = for($i = 0; $i -lt $lines.Count; $i++) {
        '{0,4}: {1}' -f ($Skip + $i + 1), $lines[$i]
    }

    $font = New-Object System.Drawing.Font('Consolas', 16)
    $titleFont = New-Object System.Drawing.Font('Segoe UI', 18, [System.Drawing.FontStyle]::Bold)
    $lineHeight = 28
    $padding = 24
    $titleHeight = 52
    $width = 1700
    $height = $padding + $titleHeight + ($numbered.Count * $lineHeight) + $padding

    $bmp = New-Object System.Drawing.Bitmap $width, $height
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.Clear([System.Drawing.Color]::White)
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::ClearTypeGridFit

    $lightGray = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(245,247,250))
    $dark = [System.Drawing.Brushes]::Black
    $muted = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(80,80,80))
    $borderPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(220,220,220))

    $g.FillRectangle($lightGray, 0, 0, $width, $height)
    $g.FillRectangle([System.Drawing.Brushes]::White, 10, 10, $width - 20, $height - 20)
    $g.DrawRectangle($borderPen, 10, 10, $width - 21, $height - 21)

    $g.DrawString($Title, $titleFont, $dark, $padding, $padding)

    $y = $padding + $titleHeight
    foreach($line in $numbered) {
        $g.DrawString($line, $font, $muted, $padding, $y)
        $y += $lineHeight
    }

    $bmp.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose()
    $bmp.Dispose()
    $font.Dispose()
    $titleFont.Dispose()
    $lightGray.Dispose()
    $muted.Dispose()
    $borderPen.Dispose()
}

$root = 'C:\Users\User\Documents\School\SwipeMate'
New-CodeScreenshot -InputPath (Join-Path $root 'SwipeMate.Api\Program.cs') -Skip 0 -First 40 -OutputPath (Join-Path $root 'Documentation\assets\code-api-startup.png') -Title 'Program.cs - конфигуриране на API, Identity и JWT'
New-CodeScreenshot -InputPath (Join-Path $root 'SwipeMate.Api\Controllers\SessionsController.cs') -Skip 32 -First 40 -OutputPath (Join-Path $root 'Documentation\assets\code-session-create.png') -Title 'SessionsController.cs - създаване на групова сесия и покани'
New-CodeScreenshot -InputPath (Join-Path $root 'SwipeMate.Mobile\Pages\HomePage.xaml') -Skip 52 -First 36 -OutputPath (Join-Path $root 'Documentation\assets\code-home-ui.png') -Title 'HomePage.xaml - категории и потребителски интерфейс'
New-CodeScreenshot -InputPath (Join-Path $root 'SwipeMate.Mobile\Pages\FriendsPage.xaml') -Skip 18 -First 28 -OutputPath (Join-Path $root 'Documentation\assets\code-friends-search.png') -Title 'FriendsPage.xaml - търсене на потребители и приятели'

Get-ChildItem (Join-Path $root 'Documentation\assets') | Select-Object Name,Length
