// A FileField stores one file: an ADF batch must become one complete document.
export async function combineScannedPages(files: File[], dpi = 200): Promise<File> {
    const { PDFDocument } = await import('pdf-lib');
    const document = await PDFDocument.create();
    const pointsPerPixel = 72 / (dpi > 0 && Number.isFinite(dpi) ? dpi : 200);

    for (const file of files) {
        const bytes = await file.arrayBuffer();
        if (file.type === 'application/pdf') {
            const source = await PDFDocument.load(bytes);
            const pages = await document.copyPages(source, source.getPageIndices());
            for (const page of pages) document.addPage(page);
        } else {
            const image = file.type === 'image/png'
                ? await document.embedPng(bytes)
                : await document.embedJpg(bytes);
            const width = image.width * pointsPerPixel;
            const height = image.height * pointsPerPixel;
            const page = document.addPage([width, height]);
            page.drawImage(image, { x: 0, y: 0, width, height });
        }
    }

    return new File([new Uint8Array(await document.save())], `scan_${Date.now()}.pdf`, { type: 'application/pdf' });
}
