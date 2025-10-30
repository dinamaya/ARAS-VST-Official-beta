window.downloadFile = (fileName, bytesBase64) => {
	const link = document.createElement('a');
	link.href = 'data:application/octet-stream;base64,' + bytesBase64;
	link.download = fileName;
	link.click();
};