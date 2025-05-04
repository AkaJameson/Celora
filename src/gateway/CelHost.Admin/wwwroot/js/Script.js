export function downloadFileFromBytes(filename, content) {
    const blob = new Blob([content]);
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = filename;
    link.click();
    URL.revokeObjectURL(link.href);
}
