// Copiar en Tests de Login / Register / Refresh en Postman
function saveAuthSession(role) {
    if (pm.response.code !== 200) {
        console.warn('Auth falló:', pm.response.code, pm.response.text());
        return;
    }
    const j = pm.response.json();
    const pairs = {
        accessToken: j.accessToken,
        refreshToken: j.refreshToken,
        userEmail: j.user?.email || '',
        userId: String(j.user?.id || ''),
        userFirstName: j.user?.firstName || '',
        sessionRole: role,
        isLoggedIn: 'true'
    };
    if (role === 'admin') pairs.adminToken = j.accessToken;
    if (role === 'customer') pairs.customerToken = j.accessToken;

    Object.entries(pairs).forEach(([key, value]) => {
        pm.collectionVariables.set(key, value);
        try { pm.environment.set(key, value); } catch (e) { /* sin entorno activo */ }
    });

    console.log('Sesión guardada [' + role + ']:', j.user?.email);
}

function clearAuthSession() {
    ['accessToken', 'refreshToken', 'adminToken', 'customerToken', 'userEmail', 'userId', 'userFirstName', 'sessionRole'].forEach((key) => {
        pm.collectionVariables.unset(key);
        try { pm.environment.unset(key); } catch (e) {}
    });
    pm.collectionVariables.set('isLoggedIn', 'false');
    try { pm.environment.set('isLoggedIn', 'false'); } catch (e) {}
    console.log('Sesión cerrada');
}
