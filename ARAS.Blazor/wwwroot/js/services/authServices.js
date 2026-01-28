window.authHelper = {
	clearAuthCookie: function () {
		const cookieName = "vstecs-aras-jwt";
		document.cookie = cookieName + "=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT; Secure; SameSite=None";
	}
};