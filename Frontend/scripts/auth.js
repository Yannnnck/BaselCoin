document.getElementById("loginForm").addEventListener("submit", async function(event) {
    event.preventDefault();

    let username = document.getElementById("username").value;
    let password = document.getElementById("password").value;

    let response = await fetch("http://localhost:5000/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password })
    });

    let data = await response.json();
    if (response.ok) {
        localStorage.setItem("token", data.token);
        alert("Login erfolgreich!");
        window.location.href = data.role === "Admin" ? "admin.html" : "dashboard.html";
    } else {
        document.getElementById("errorMessage").innerText = data.message;
    }
});

function logout() {
    let token = localStorage.getItem("token");
    if (token) {
        fetch("http://localhost:5000/api/auth/logout", {
            method: "POST",
            headers: { "Authorization": `Bearer ${token}` }
        });
    }
    localStorage.removeItem("token");
    window.location.href = "index.html";
}

// Auto-Logout nach 10 Minuten Inaktivität
let idleTime = 0;
setInterval(() => { idleTime++; if (idleTime >= 10) logout(); }, 60000);

document.onmousemove = document.onkeypress = () => idleTime = 0;

function logout() {
    localStorage.removeItem("token");
    window.location.href = "index.html";
}

