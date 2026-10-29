window.scrollToElement = (elementId) => {
	const element = document.getElementById(elementId);
	if (element) {
		element.scrollIntoView({ behavior: 'smooth', block: 'start' });
	}
};

// Asks for confirmation before the tab is closed or refreshed while a
// posting to Oracle Fusion is in progress (used by FusionPostingOverlay).
window.arasPostingGuard = (function () {
	let enabled = false;
	const handler = (event) => {
		event.preventDefault();
		event.returnValue = '';
		return '';
	};

	return {
		enable() {
			if (!enabled) {
				window.addEventListener('beforeunload', handler);
				enabled = true;
			}
		},
		disable() {
			if (enabled) {
				window.removeEventListener('beforeunload', handler);
				enabled = false;
			}
		}
	};
})();
