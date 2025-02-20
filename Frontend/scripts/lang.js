const translations = {
    de: {
        login: "Anmelden",
        username: "Benutzername",
        password: "Passwort",
        error: "Ungültige Anmeldedaten"
    },
    en: {
        login: "Login",
        username: "Username",
        password: "Password",
        error: "Invalid login credentials"
    },
    fr: {
        login: "Connexion",
        username: "Nom d'utilisateur",
        password: "Mot de passe",
        error: "Identifiants invalides"
    },
    it: {
        login: "Accesso",
        username: "Nome utente",
        password: "Password",
        error: "Credenziali non valide"
    }
};

// 🌍 Sprache setzen
function setLanguage(lang) {
    localStorage.setItem("language", lang);
    applyLanguage();
}

// 🔄 Sprache anwenden
function applyLanguage() {
    let lang = localStorage.getItem("language") || "de";
    document.getElementById("loginButton").innerText = translations[lang].login;
    document.getElementById("username").placeholder = translations[lang].username;
    document.getElementById("password").placeholder = translations[lang].password;
}

// 🌍 Sprache beim Laden anwenden
document.addEventListener("DOMContentLoaded", applyLanguage);
