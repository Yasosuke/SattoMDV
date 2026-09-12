# Regenerate the original SattoMDV icon using Windows System.Drawing.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$assetRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../src/MDV/Assets'))
function Draw-Icon([int]$size) {
    $bitmap = [Drawing.Bitmap]::new($size,$size)
    $g = [Drawing.Graphics]::FromImage($bitmap)
    $g.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.ScaleTransform($size / 256.0, $size / 256.0)
    $tile = [Drawing.Drawing2D.GraphicsPath]::new()
    $tile.AddArc(8,8,80,80,180,90)
    $tile.AddArc(168,8,80,80,270,90)
    $tile.AddArc(168,168,80,80,0,90)
    $tile.AddArc(8,168,80,80,90,90)
    $tile.CloseFigure()
    $teal = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#146C70'))
    $paper = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#FFF9EA'))
    $fold = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#A6DAD0'))
    $g.FillPath($teal,$tile)
    $points = [Drawing.PointF[]]@([Drawing.PointF]::new(105,48),[Drawing.PointF]::new(173,48),[Drawing.PointF]::new(207,83),[Drawing.PointF]::new(184,207),[Drawing.PointF]::new(76,207))
    $g.FillPolygon($paper,$points)
    $g.FillPolygon($fold,[Drawing.PointF[]]@([Drawing.PointF]::new(173,48),[Drawing.PointF]::new(167,83),[Drawing.PointF]::new(207,83)))
    $ink = [Drawing.Pen]::new([Drawing.ColorTranslator]::FromHtml('#146C70'),11)
    $ink.StartCap = $ink.EndCap = [Drawing.Drawing2D.LineCap]::Round
    $g.DrawLine($ink,113,111,170,111)
    $g.DrawLine($ink,108,139,165,139)
    $g.DrawLine($ink,103,167,142,167)
    $speed = [Drawing.Pen]::new([Drawing.ColorTranslator]::FromHtml('#A6DAD0'),11)
    $speed.StartCap = $speed.EndCap = [Drawing.Drawing2D.LineCap]::Round
    $g.DrawLine($speed,43,94,77,94)
    $g.DrawLine($speed,31,124,69,124)
    $g.DrawLine($speed,39,154,61,154)
    $g.Dispose(); $tile.Dispose(); $teal.Dispose(); $paper.Dispose(); $fold.Dispose(); $ink.Dispose(); $speed.Dispose()
    return $bitmap
}
$preview=Draw-Icon 512
$preview.Save((Join-Path $assetRoot 'SattoMDV.png'),[Drawing.Imaging.ImageFormat]::Png)
$preview.Dispose()
$sizes=@(16,20,24,32,40,48,64,128,256)
$images=@(foreach($size in $sizes) {
    $bitmap=Draw-Icon $size
    $stream=[IO.MemoryStream]::new()
    $bitmap.Save($stream,[Drawing.Imaging.ImageFormat]::Png)
    ,$stream.ToArray()
    $stream.Dispose(); $bitmap.Dispose()
})
$file=[IO.File]::Create((Join-Path $assetRoot 'SattoMDV.ico'))
$writer=[IO.BinaryWriter]::new($file)
try {
    $writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]$sizes.Count)
    $offset=6+16*$sizes.Count
    for($i=0;$i -lt $sizes.Count;$i++) {
        $dimension=if($sizes[$i] -eq 256){0}else{$sizes[$i]}
        $writer.Write([byte]$dimension); $writer.Write([byte]$dimension)
        $writer.Write([byte]0); $writer.Write([byte]0)
        $writer.Write([uint16]1); $writer.Write([uint16]32)
        $writer.Write([uint32]$images[$i].Length); $writer.Write([uint32]$offset)
        $offset+=$images[$i].Length
    }
    foreach($bytes in $images){$writer.Write([byte[]]$bytes)}
} finally { $writer.Dispose(); $file.Dispose() }
