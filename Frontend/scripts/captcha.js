let captchaText = "";

function generateCaptcha() {
    let canvas = document.getElementById("captchaCanvas");
    let ctx = canvas.getContext("2d");
    let chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    captchaText = "";

    // Hintergrundfarbe setzen
    ctx.fillStyle = "#f4f4f4";
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    // Zufälligen Captcha-Text generieren
    for (let i = 0; i < 5; i++) {
        captchaText += chars.charAt(Math.floor(Math.random() * chars.length));
    }

    // Verzerrte Schrift zeichnen
    ctx.font = "20px Arial";
    ctx.fillStyle = "#333";
    ctx.setTransform(1, 0.2 - Math.random() * 0.4, 0, 1, 10, 20);
    ctx.fillText(captchaText, 10, 20);

    // Zufällige Störungen einfügen
    for (let i = 0; i < 5; i++) {
        ctx.strokeStyle = "#999";
        ctx.beginPath();
        ctx.moveTo(Math.random() * canvas.width, Math.random() * canvas.height);
        ctx.lineTo(Math.random() * canvas.width, Math.random() * canvas.height);
        ctx.stroke();
    }
}

document.addEventListener("DOMContentLoaded", generateCaptcha);
