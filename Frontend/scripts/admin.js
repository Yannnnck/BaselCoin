async function createUser() {
    let username = document.getElementById("newUsername").value;
    let password = document.getElementById("newPassword").value;

    let token = localStorage.getItem("token");
    let response = await fetch("http://localhost:5000/api/admin/createUser", {
        method: "POST",
        headers: { 
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        },
        body: JSON.stringify({ username, password })
    });

    let data = await response.json();
    alert(data.message);
    loadUsers(); // Benutzerliste aktualisieren
}

// 📌 Benutzerliste laden
async function loadUsers() {
    let token = localStorage.getItem("token");
    let response = await fetch("http://localhost:5000/api/admin/getUsers", {
        method: "GET",
        headers: { "Authorization": `Bearer ${token}` }
    });

    let users = await response.json();
    let userList = document.getElementById("userList");
    userList.innerHTML = "";

    users.forEach(user => {
        let li = document.createElement("li");
        li.innerHTML = `${user.username} <button onclick="deleteUser('${user.username}')">Löschen</button>`;
        userList.appendChild(li);
    });
}

// 📌 Benutzer löschen
async function deleteUser(username) {
    let token = localStorage.getItem("token");
    let response = await fetch(`http://localhost:5000/api/admin/deleteUser/${username}`, {
        method: "DELETE",
        headers: { "Authorization": `Bearer ${token}` }
    });

    let data = await response.json();
    alert(data.message);
    loadUsers();
}

// Beim Laden des Admin-Dashboards Benutzerliste abrufen
document.addEventListener("DOMContentLoaded", loadUsers);
