$docDir = 'C:\Users\User\Documents\School\SwipeMate\Documentation'
$mdPath = (Get-ChildItem -Path $docDir -Filter 'SwipeMate_*.md' | Select-Object -First 1 -ExpandProperty FullName)
$htmlPath = Join-Path $docDir 'SwipeMate_Diploma.html'
$lines = Get-Content $mdPath -Encoding utf8

function Convert-Inline([string]$text) {
    $text = [System.Net.WebUtility]::HtmlEncode($text)
    $text = $text -replace '\*\*(.+?)\*\*', '<strong>$1</strong>'
    $text = $text -replace '\*(.+?)\*', '<em>$1</em>'
    $text = $text -replace '`([^`]+)`', '<code>$1</code>'
    $text = $text -replace '!\[(.*?)\]\((.*?)\)', '<figure><img src="$2" alt="$1" /><figcaption>$1</figcaption></figure>'
    return $text
}

$body = New-Object System.Text.StringBuilder
$inUl = $false
$inOl = $false
$inTitlePage = $false
$schoolHeaderHtml = @'
<div class="school-header">
  <img class="school-logo" src="assets/school-header.jpg" alt="Лого на гимназията" />
  <div class="school-header-text">
    <div class="school-name">МАТЕМАТИЧЕСКА ГИМНАЗИЯ „АКАДЕМИК КИРИЛ ПОПОВ”</div>
    <div class="school-contact">4001 Пловдив, ул. „Чемшир” № 11, тел.: +359 32 643 157; e-mail: omg@omg-bg.com , www.omg-bg.com</div>
  </div>
</div>
'@

foreach($raw in $lines) {
    $line = $raw.TrimEnd()
    $trim = $line.Trim()

    if($trim -eq ':::titlepage') {
        if($inUl){ [void]$body.AppendLine('</ul>'); $inUl=$false }
        if($inOl){ [void]$body.AppendLine('</ol>'); $inOl=$false }
        [void]$body.AppendLine('<section class="title-page">')
        [void]$body.AppendLine($schoolHeaderHtml)
        $inTitlePage = $true
        continue
    }

    if($trim -eq ':::/titlepage') {
        if($inUl){ [void]$body.AppendLine('</ul>'); $inUl=$false }
        if($inOl){ [void]$body.AppendLine('</ol>'); $inOl=$false }
        [void]$body.AppendLine('</section>')
        $inTitlePage = $false
        continue
    }

    if($trim -eq ':::pagebreak') {
        if($inUl){ [void]$body.AppendLine('</ul>'); $inUl=$false }
        if($inOl){ [void]$body.AppendLine('</ol>'); $inOl=$false }
        [void]$body.AppendLine('<div class="page-break"></div>')
        continue
    }

    if([string]::IsNullOrWhiteSpace($trim)) {
        if($inUl){ [void]$body.AppendLine('</ul>'); $inUl=$false }
        if($inOl){ [void]$body.AppendLine('</ol>'); $inOl=$false }
        continue
    }

    if($trim -eq '---') {
        if($inUl){ [void]$body.AppendLine('</ul>'); $inUl=$false }
        if($inOl){ [void]$body.AppendLine('</ol>'); $inOl=$false }
        [void]$body.AppendLine('<hr />')
        continue
    }

    if($trim -match '^# (.+)$') {
        if($inUl){ [void]$body.AppendLine('</ul>'); $inUl=$false }
        if($inOl){ [void]$body.AppendLine('</ol>'); $inOl=$false }
        $class = if($inTitlePage){ ' class="title-main"' } else { '' }
        [void]$body.AppendLine("<h1$class>" + (Convert-Inline $matches[1]) + '</h1>')
        continue
    }

    if($trim -match '^## (.+)$') {
        if($inUl){ [void]$body.AppendLine('</ul>'); $inUl=$false }
        if($inOl){ [void]$body.AppendLine('</ol>'); $inOl=$false }
        $class = if($inTitlePage){ ' class="title-sub"' } else { '' }
        [void]$body.AppendLine("<h2$class>" + (Convert-Inline $matches[1]) + '</h2>')
        continue
    }

    if($trim -match '^### (.+)$') {
        if($inUl){ [void]$body.AppendLine('</ul>'); $inUl=$false }
        if($inOl){ [void]$body.AppendLine('</ol>'); $inOl=$false }
        [void]$body.AppendLine('<h3>' + (Convert-Inline $matches[1]) + '</h3>')
        continue
    }

    if($trim -match '^\d+\. (.+)$') {
        if($inUl){ [void]$body.AppendLine('</ul>'); $inUl=$false }
        if(-not $inOl){ [void]$body.AppendLine('<ol>'); $inOl=$true }
        [void]$body.AppendLine('<li>' + (Convert-Inline $matches[1]) + '</li>')
        continue
    }

    if($trim -match '^- (.+)$') {
        if($inOl){ [void]$body.AppendLine('</ol>'); $inOl=$false }
        if(-not $inUl){ [void]$body.AppendLine('<ul>'); $inUl=$true }
        [void]$body.AppendLine('<li>' + (Convert-Inline $matches[1]) + '</li>')
        continue
    }

    $converted = Convert-Inline $trim
    if($trim -match '^</?(table|thead|tbody|tr|td|th|caption)[ >]?' -or $trim -eq '</table>') {
        if($inUl){ [void]$body.AppendLine('</ul>'); $inUl=$false }
        if($inOl){ [void]$body.AppendLine('</ol>'); $inOl=$false }
        [void]$body.AppendLine($trim)
        continue
    }
    if($converted -like '<figure>*') {
        if($inUl){ [void]$body.AppendLine('</ul>'); $inUl=$false }
        if($inOl){ [void]$body.AppendLine('</ol>'); $inOl=$false }
        [void]$body.AppendLine($converted)
        continue
    }

    $pClass = if($inTitlePage){ ' class="title-line"' } else { '' }
    [void]$body.AppendLine("<p$pClass>" + $converted + '</p>')
}

if($inUl){ [void]$body.AppendLine('</ul>') }
if($inOl){ [void]$body.AppendLine('</ol>') }

$html = @"
<!DOCTYPE html>
<html lang="bg">
<head>
<meta charset="utf-8" />
<title>SwipeMate Diploma Documentation</title>
<style>
@page {
    size: A4;
    margin: 2cm 2cm 2cm 2.2cm;
}

body {
    margin: 0;
    padding: 0;
    background: #f1eef4;
    color: #111;
    font-family: 'Times New Roman', Times, serif;
    font-size: 12pt;
}

.page {
    width: 170mm;
    margin: 10mm auto;
    background: #fff;
    padding: 18mm 18mm 20mm 18mm;
    box-shadow: 0 0 10px rgba(0,0,0,.08);
    position: relative;
}

.title-page {
    min-height: 245mm;
    display: flex;
    flex-direction: column;
    justify-content: flex-start;
    text-align: center;
}

.school-header {
    display: grid;
    grid-template-columns: 24mm 1fr;
    align-items: center;
    column-gap: 8mm;
    width: 100%;
    margin-bottom: 18mm;
}

.school-logo {
    width: 18mm;
    height: auto;
    justify-self: start;
}

.school-header-text {
    text-align: center;
}

.school-name {
    font-size: 15pt;
    font-weight: bold;
    color: #2e5d90;
    text-decoration: underline;
    margin-bottom: 4mm;
}

.school-contact {
    font-size: 10.5pt;
    color: #2e5d90;
}

.title-main {
    margin-top: 18mm;
    margin-bottom: 8mm;
    font-size: 20pt;
    text-align: center;
}

.title-sub {
    margin-top: 0;
    margin-bottom: 4mm;
    font-size: 16pt;
    text-align: center;
    border-bottom: none;
}

.title-line {
    text-align: center;
    text-indent: 0;
    margin: 1.8mm 0;
}

h1 {
    font-size: 16pt;
    margin-top: 10mm;
    margin-bottom: 4mm;
    text-align: center;
}

h2 {
    font-size: 14pt;
    margin-top: 8mm;
    margin-bottom: 3mm;
}

h3 {
    font-size: 13pt;
    margin-top: 6mm;
    margin-bottom: 2mm;
}

p {
    margin: 0 0 4mm 0;
    line-height: 1.45;
    text-align: justify;
    text-indent: 1.25cm;
}

ol, ul {
    margin-top: 2mm;
    margin-bottom: 4mm;
    padding-left: 1.1cm;
}

li {
    margin-bottom: 2mm;
    line-height: 1.4;
    text-align: justify;
}

table {
    width: 100%;
    border-collapse: collapse;
    margin: 4mm 0 6mm 0;
    font-size: 11pt;
}

caption {
    caption-side: top;
    text-align: left;
    font-style: italic;
    margin-bottom: 2mm;
    color: #395a86;
}

th, td {
    border: 1px solid #888;
    padding: 2.2mm 2.5mm;
    vertical-align: top;
    text-align: left;
}

th {
    background: #f2f4f8;
    font-weight: bold;
}

code {
    font-family: Consolas, monospace;
    font-size: 10.5pt;
    background: #f4f4f4;
    padding: 0 2px;
}

hr {
    border: none;
    border-top: 1px solid #aaa;
    margin: 6mm 0;
}

figure {
    margin: 5mm 0 6mm 0;
    text-align: center;
}

figure img {
    max-width: 100%;
    border: 1px solid #cfcfcf;
}

figcaption {
    font-size: 10pt;
    margin-top: 2mm;
    text-align: center;
}

.page-break {
    page-break-before: always;
    break-before: page;
    height: 0;
}

@media print {
    body {
        background: #fff;
    }

    .page {
        width: auto;
        margin: 0;
        padding: 0;
        box-shadow: none;
    }

    .title-page {
        margin-top: 10mm;
    }
}
</style>
</head>
<body>
<div class="page">
$($body.ToString())
</div>
</body>
</html>
"@

Set-Content -Path $htmlPath -Value $html -Encoding utf8

