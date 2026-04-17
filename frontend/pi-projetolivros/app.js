// ============================================
// PI-Letrify — app.js
// ============================================

const API_BASE = 'http://localhost:5265';

// Global state
window._bookMap = new Map();
window._currentProfile = null;
window._searchResults = [];

// ===================== AUTH =====================

function getToken() { return localStorage.getItem('letrify_token'); }
function getUserId() { return localStorage.getItem('letrify_userId'); }
function getUserName() { return localStorage.getItem('letrify_userName'); }
function isLoggedIn() { return !!getToken(); }

function decodeJWT(token) {
    try {
        const payload = token.split('.')[1];
        return JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
    } catch { return null; }
}

function saveSession(token) {
    const p = decodeJWT(token);
    if (!p) return;
    localStorage.setItem('letrify_token', token);
    localStorage.setItem('letrify_userId', p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || '');
    localStorage.setItem('letrify_userName', p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname'] || '');
    localStorage.setItem('letrify_userEmail', p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || '');
}

function logout() {
    localStorage.removeItem('letrify_token');
    localStorage.removeItem('letrify_userId');
    localStorage.removeItem('letrify_userName');
    localStorage.removeItem('letrify_userEmail');
    navigate('login');
}

// ===================== API HELPERS =====================

function authHeaders() {
    return { 'Authorization': `Bearer ${getToken()}`, 'Content-Type': 'application/json' };
}

async function apiRequest(url, options = {}) {
    const res = await fetch(`${API_BASE}${url}`, options);
    // Handle expired token
    if (res.status === 401) {
        showToast('Sessão expirada. Faça login novamente.', 'error');
        logout();
        throw new Error('Sessão expirada');
    }
    let data;
    const text = await res.text();
    try { data = JSON.parse(text); } catch { data = text; }
    if (!res.ok) {
        const msg = typeof data === 'object' ? (data.message || data.title || JSON.stringify(data)) : data;
        throw new Error(msg || `Erro ${res.status}`);
    }
    return data;
}

function apiGet(url) { return apiRequest(url); }
function apiGetAuth(url) { return apiRequest(url, { headers: authHeaders() }); }

function apiPost(url, body) {
    return apiRequest(url, { method: 'POST', headers: authHeaders(), body: JSON.stringify(body) });
}

function apiDelete(url) {
    return apiRequest(url, { method: 'DELETE', headers: authHeaders() });
}

function apiPutFormData(url, formData) {
    return apiRequest(url, {
        method: 'PUT',
        headers: { 'Authorization': `Bearer ${getToken()}` },
        body: formData
    });
}

async function apiLogin(email, senha) {
    return apiRequest('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, senha })
    });
}

async function apiRegister(nome, email, senha) {
    return apiRequest('/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ nome, email, senha })
    });
}

// ===================== ROUTER =====================

function navigate(view) { window.location.hash = '#' + view; }
function getRoute() { return window.location.hash.replace('#', '') || 'login'; }

function initRouter() {
    window.addEventListener('hashchange', handleRoute);
    handleRoute();
}

function handleRoute() {
    const route = getRoute();
    const publicRoutes = ['login', 'register'];

    if (!isLoggedIn() && !publicRoutes.includes(route)) { navigate('login'); return; }
    if (isLoggedIn() && publicRoutes.includes(route)) { navigate('perfil'); return; }

    // Hide all views
    document.querySelectorAll('.view').forEach(v => v.classList.add('d-none'));

    // Show/hide navbar
    const navbar = document.getElementById('mainNavbar');
    if (navbar) navbar.classList.toggle('d-none', !isLoggedIn());

    // Show target view
    const viewEl = document.getElementById('view-' + route);
    if (viewEl) {
        viewEl.classList.remove('d-none');
        if (route === 'perfil') loadProfileView();
    }

    // Update active nav link
    document.querySelectorAll('.nav-link-item').forEach(link => {
        link.classList.toggle('active', link.dataset.view === route);
    });

    // Update user name in navbar
    const navUser = document.getElementById('navUserName');
    if (navUser) navUser.textContent = getUserName() || 'Usuário';
}

// ===================== PROFILE =====================

async function loadProfileView() {
    const userId = getUserId();
    if (!userId) return;

    try {
        const [profile, bookshelf] = await Promise.all([
            apiGet(`/api/usuario/${userId}`),
            apiGet(`/api/usuario/${userId}/livros`)
        ]);
        renderProfile(profile);
        renderBookshelf(bookshelf);
    } catch (err) {
        console.error('Erro ao carregar perfil:', err);
        showToast('Erro ao carregar perfil. Verifique se o backend está rodando.', 'error');
    }
}

function renderProfile(profile) {
    const avatarUrl = profile.fotoPerfil ? `${API_BASE}${profile.fotoPerfil}` : null;
    const avatarEl = document.getElementById('profileAvatar');
    if (avatarEl) {
        avatarEl.innerHTML = avatarUrl
            ? `<img src="${avatarUrl}" alt="Avatar">`
            : `<div class="avatar-placeholder"><i class="bi bi-person-fill"></i></div>`;
    }

    setText('profileName', profile.nome || getUserName() || 'Usuário');
    const cityEl = document.getElementById('profileCity');
    if (cityEl) {
        if (profile.cidade) {
            cityEl.innerHTML = `<i class="bi bi-geo-alt-fill"></i> ${profile.cidade}`;
            cityEl.style.display = '';
        } else {
            cityEl.style.display = 'none';
        }
    }
    const bioEl = document.getElementById('profileBio');
    if (bioEl) {
        bioEl.textContent = profile.descricao || '';
        bioEl.style.display = profile.descricao ? '' : 'none';
    }

    window._currentProfile = profile;
}

function renderBookshelf(shelf) {
    const lendo = shelf.lendo || [];
    const lido = shelf.lido || [];
    const queroLer = shelf.queroLer || [];
    const total = lendo.length + lido.length + queroLer.length;

    // Store books in map
    window._bookMap.clear();
    [...lendo, ...lido, ...queroLer].forEach(b => window._bookMap.set(b.id, b));

    // Stats
    setText('statTotal', total);
    setText('statLendo', lendo.length);
    setText('statLido', lido.length);

    // Tab counts
    setText('tabCountLendo', lendo.length);
    setText('tabCountLido', lido.length);
    setText('tabCountQueroLer', queroLer.length);

    // Render book lists
    renderBookList('listLendo', lendo, 'Lendo');
    renderBookList('listLido', lido, 'Lido');
    renderBookList('listQueroLer', queroLer, 'Quero Ler');

    // Show default tab
    showShelfTab('lendo');
}

function renderBookList(containerId, books, status) {
    const container = document.getElementById(containerId);
    if (!container) return;

    if (books.length === 0) {
        container.innerHTML = `
            <div class="empty-shelf">
                <i class="bi bi-journal-bookmark"></i>
                <p>Nenhum livro nesta categoria ainda</p>
                <button class="btn-add-shelf" onclick="openAddBookModal()">
                    <i class="bi bi-plus-lg"></i> Adicionar livro
                </button>
            </div>
        `;
        return;
    }

    container.innerHTML = books.map(book => createShelfBookCard(book, status)).join('');
}

function createShelfBookCard(book, status) {
    const isbn = book.isbn && book.isbn !== 'Sem ISBN' ? book.isbn : null;
    const coverUrl = isbn ? `https://covers.openlibrary.org/b/isbn/${isbn}-M.jpg` : null;

    const allStatuses = ['Lendo', 'Lido', 'Quero Ler'];
    const otherStatuses = allStatuses.filter(s => s !== status);

    const statusIcons = { 'Lendo': 'book-half', 'Lido': 'check2-circle', 'Quero Ler': 'bookmark-star' };
    const initial = (book.titulo || '?').charAt(0).toUpperCase();

    return `
        <div class="shelf-book-card">
            <div class="shelf-book-cover">
                ${coverUrl
                    ? `<img src="${coverUrl}" alt="Capa" onload="handleCoverLoad(this)" onerror="this.style.display='none';this.nextElementSibling.style.display='flex'">`
                    : ''}
                <div class="cover-placeholder-sm" ${coverUrl ? 'style="display:none"' : ''}>
                    <span style="font-weight:800;font-size:1.6rem">${initial}</span>
                </div>
            </div>
            <div class="shelf-book-info">
                <div class="shelf-book-title" title="${book.titulo || 'Sem título'}">${book.titulo || 'Sem título'}</div>
                <div class="shelf-book-author">${book.autor || 'Autor desconhecido'}</div>
                ${isbn ? `<div class="shelf-book-isbn"><i class="bi bi-upc-scan"></i> ${isbn}</div>` : ''}
                <div class="shelf-book-actions">
                    ${otherStatuses.map(s => `
                        <button class="btn-move" onclick="moveBook(${book.id}, '${s}')" title="Mover para ${s}">
                            <i class="bi bi-${statusIcons[s]}"></i> ${s}
                        </button>
                    `).join('')}
                    <button class="btn-remove" onclick="removeBook(${book.id})" title="Remover da estante">
                        <i class="bi bi-trash3"></i>
                    </button>
                </div>
            </div>
        </div>
    `;
}

function showShelfTab(tab) {
    document.querySelectorAll('.shelf-tab').forEach(t => t.classList.remove('active'));
    const activeTab = document.querySelector(`.shelf-tab[data-tab="${tab}"]`);
    if (activeTab) activeTab.classList.add('active');

    document.querySelectorAll('.shelf-list').forEach(l => l.classList.add('d-none'));
    const listMapping = { lendo: 'listLendo', lido: 'listLido', queroLer: 'listQueroLer' };
    const activeList = document.getElementById(listMapping[tab]);
    if (activeList) activeList.classList.remove('d-none');
}

// ===================== BOOK ACTIONS =====================

async function moveBook(bookId, newStatus) {
    const book = window._bookMap.get(bookId);
    if (!book) return;

    try {
        await apiPost('/api/usuario/meus-livros', {
            titulo: book.titulo,
            autor: book.autor,
            isbn: book.isbn,
            status: newStatus
        });
        showToast(`Livro movido para "${newStatus}"!`, 'success');
        loadProfileView();
    } catch (err) {
        showToast(`Erro: ${err.message}`, 'error');
    }
}

async function removeBook(bookId) {
    if (!confirm('Remover este livro da sua estante?')) return;
    try {
        await apiDelete(`/api/usuario/meus-livros/${bookId}`);
        showToast('Livro removido!', 'success');
        loadProfileView();
    } catch (err) {
        showToast(`Erro: ${err.message}`, 'error');
    }
}

// ===================== ADD BOOK (SEARCH) =====================

async function searchBooksToAdd() {
    const query = document.getElementById('addBookSearch').value.trim();
    if (!query) return;

    const searchType = document.getElementById('searchType').value;
    const sortOrder = document.getElementById('searchSort').value;
    const qty = document.getElementById('searchQty').value || '20';

    const container = document.getElementById('searchResults');
    container.innerHTML = '<div class="search-loading"><div class="spinner-sm"></div> Buscando livros...</div>';

    try {
        // Build API URL based on search type
        let url;
        if (searchType === 'autor') {
            url = `/api/livro/livrosautor?autor=${encodeURIComponent(query)}&quantidade=${qty}`;
        } else if (searchType === 'tema') {
            url = `/api/livro/livrostema?tema=${encodeURIComponent(query)}&quantidade=${qty}`;
        } else {
            url = `/api/livro/livrostitulo?titulo=${encodeURIComponent(query)}&quantidade=${qty}`;
        }

        // Add sort parameter if selected
        if (sortOrder) url += `&sort=${sortOrder}`;

        const data = await apiGet(url);
        const books = Array.isArray(data) ? data : [];

        // Filter: only books WITH a valid ISBN (ensures covers + avoids DB unique constraint)
        // Deduplicate by ISBN
        const seenIsbns = new Set();
        window._searchResults = [];

        for (const book of books) {
            const isbn = book.isbn && book.isbn !== 'Sem ISBN' ? book.isbn.trim() : '';
            if (!isbn || seenIsbns.has(isbn)) continue;
            seenIsbns.add(isbn);
            window._searchResults.push({
                titulo: book.titulo || 'Sem título',
                autor: book.autorPrincipal || book.autor || 'Desconhecido',
                isbn: isbn
            });
        }

        if (window._searchResults.length === 0) {
            container.innerHTML = '<p class="search-empty"><i class="bi bi-search" style="font-size:1.5rem;display:block;margin-bottom:8px;opacity:0.5;"></i>Nenhum livro com ISBN encontrado. Tente outro termo ou tipo de busca.</p>';
            return;
        }

        container.innerHTML = window._searchResults.map((sr, i) => {
            const initials = sr.titulo.charAt(0).toUpperCase();
            return `
            <div class="search-result-item">
                <div class="search-result-cover">
                    <img src="https://covers.openlibrary.org/b/isbn/${sr.isbn}-S.jpg" alt=""
                         onload="handleCoverLoad(this)"
                         onerror="this.style.display='none';this.nextElementSibling.style.display='flex'">
                    <span class="cover-fallback-sm" style="display:none">${initials}</span>
                </div>
                <div class="search-result-info">
                    <div class="search-result-title" title="${sr.titulo}">${sr.titulo}</div>
                    <div class="search-result-author">${sr.autor}</div>
                    <div class="search-result-isbn">ISBN: ${sr.isbn}</div>
                </div>
                <div class="search-result-actions">
                    <select class="status-select" id="searchStatus${i}">
                        <option value="Lendo">📖 Lendo</option>
                        <option value="Lido">✅ Já li</option>
                        <option value="Quero Ler">📋 Quero ler</option>
                    </select>
                    <button class="btn-add-book" id="searchBtn${i}" onclick="addBookToShelf(${i})">
                        <i class="bi bi-plus-lg"></i> Adicionar
                    </button>
                </div>
            </div>
        `}).join('');
    } catch (err) {
        container.innerHTML = `<p class="search-empty">Erro na busca: ${err.message}</p>`;
    }
}

async function addBookToShelf(index) {
    const book = window._searchResults[index];
    if (!book) return;

    const selectEl = document.getElementById(`searchStatus${index}`);
    const btnEl = document.getElementById(`searchBtn${index}`);
    const status = selectEl.value;

    try {
        btnEl.disabled = true;
        btnEl.innerHTML = '<div class="spinner-sm"></div>';
        await apiPost('/api/usuario/meus-livros', {
            titulo: book.titulo,
            autor: book.autor,
            isbn: book.isbn,
            status: status
        });
        btnEl.innerHTML = '<i class="bi bi-check-lg"></i> Adicionado';
        btnEl.classList.add('added');
        showToast(`"${book.titulo}" adicionado como "${status}"!`, 'success');
        loadProfileView();
    } catch (err) {
        btnEl.disabled = false;
        btnEl.innerHTML = '<i class="bi bi-plus-lg"></i> Adicionar';
        // Friendly message for duplicate isbn constraint
        const msg = err.message.includes('duplicate') || err.message.includes('unique')
            ? 'Este livro já existe na base. Tente movê-lo pela estante.'
            : err.message;
        showToast(`Erro: ${msg}`, 'error');
    }
}

function openAddBookModal() {
    document.getElementById('addBookModal').classList.add('show');
    document.getElementById('addBookSearch').value = '';
    document.getElementById('searchResults').innerHTML = '';
    setTimeout(() => document.getElementById('addBookSearch').focus(), 200);
}

function closeAddBookModal() {
    document.getElementById('addBookModal').classList.remove('show');
}

// ===================== EDIT PROFILE =====================

function openEditProfileModal() {
    const p = window._currentProfile || {};
    document.getElementById('editIdade').value = p.idade || '';
    document.getElementById('editCidade').value = p.cidade || '';
    document.getElementById('editDescricao').value = p.descricao || '';
    const preview = document.getElementById('editFotoPreview');
    if (p.fotoPerfil) {
        preview.src = `${API_BASE}${p.fotoPerfil}`;
        preview.style.display = 'block';
    } else {
        preview.style.display = 'none';
    }
    document.getElementById('editProfileModal').classList.add('show');
}

function closeEditProfileModal() {
    document.getElementById('editProfileModal').classList.remove('show');
}

function previewEditPhoto(input) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = (e) => {
            const preview = document.getElementById('editFotoPreview');
            preview.src = e.target.result;
            preview.style.display = 'block';
        };
        reader.readAsDataURL(input.files[0]);
    }
}

async function saveProfile() {
    const formData = new FormData();
    const idade = document.getElementById('editIdade').value;
    const cidade = document.getElementById('editCidade').value;
    const descricao = document.getElementById('editDescricao').value;
    const fotoInput = document.getElementById('editFoto');

    if (idade) formData.append('Idade', parseInt(idade));
    if (cidade) formData.append('Cidade', cidade);
    if (descricao) formData.append('Descricao', descricao);
    if (fotoInput.files[0]) formData.append('Foto', fotoInput.files[0]);

    const btn = document.getElementById('saveProfileBtn');
    try {
        btn.disabled = true;
        btn.textContent = 'Salvando...';
        await apiPutFormData('/api/usuario/editar', formData);
        showToast('Perfil atualizado com sucesso!', 'success');
        closeEditProfileModal();
        loadProfileView();
    } catch (err) {
        showToast(`Erro: ${err.message}`, 'error');
    } finally {
        btn.disabled = false;
        btn.textContent = 'Salvar Alterações';
    }
}

async function deleteAccount() {
    if (!confirm('Tem certeza absoluta que deseja excluir sua conta? Esta ação não pode ser desfeita e todos os seus livros serão apagados.')) {
        return;
    }

    try {
        await apiDelete('/api/usuario/deletar');
        showToast('Conta excluída com sucesso.', 'success');
        logout();
    } catch (err) {
        showToast(`Erro ao excluir conta: ${err.message}`, 'error');
    }
}

// ===================== AUTH FORMS =====================

async function handleLogin(e) {
    e.preventDefault();
    const email = document.getElementById('loginEmail').value;
    const senha = document.getElementById('loginSenha').value;
    const btn = document.getElementById('loginBtn');

    try {
        btn.disabled = true;
        btn.textContent = 'Entrando...';
        const data = await apiLogin(email, senha);
        saveSession(data.token);
        showToast('Bem-vindo de volta!', 'success');
        navigate('perfil');
    } catch (err) {
        showToast(err.message, 'error');
    } finally {
        btn.disabled = false;
        btn.textContent = 'Entrar';
    }
}

async function handleRegister(e) {
    e.preventDefault();
    const nome = document.getElementById('regNome').value;
    const email = document.getElementById('regEmail').value;
    const senha = document.getElementById('regSenha').value;
    const btn = document.getElementById('registerBtn');

    try {
        btn.disabled = true;
        btn.textContent = 'Criando conta...';
        await apiRegister(nome, email, senha);
        showToast('Conta criada com sucesso! Faça login.', 'success');
        navigate('login');
    } catch (err) {
        showToast(err.message, 'error');
    } finally {
        btn.disabled = false;
        btn.textContent = 'Criar Conta';
    }
}

// ===================== UTILITIES =====================

// Detects blank 1x1 pixel placeholder images from Open Library
function handleCoverLoad(img) {
    if (img.naturalWidth <= 1 || img.naturalHeight <= 1) {
        img.style.display = 'none';
        if (img.nextElementSibling) img.nextElementSibling.style.display = 'flex';
    }
}

function setText(id, text) {
    const el = document.getElementById(id);
    if (el) el.textContent = text;
}

function showToast(message, type = 'info') {
    const container = document.getElementById('toastContainer');
    if (!container) return;
    const toast = document.createElement('div');
    toast.className = `toast-msg toast-${type}`;
    toast.innerHTML = `
        <i class="bi bi-${type === 'success' ? 'check-circle-fill' : type === 'error' ? 'exclamation-triangle-fill' : 'info-circle-fill'}"></i>
        <span>${message}</span>
    `;
    container.appendChild(toast);
    requestAnimationFrame(() => toast.classList.add('show'));
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 3500);
}

// Close modals on overlay click
function handleModalOverlayClick(e, modalId) {
    if (e.target.id === modalId) {
        document.getElementById(modalId).classList.remove('show');
    }
}

// ===================== DARK MODE =====================

function toggleTheme() {
    const body = document.body;
    const isDark = body.getAttribute('data-theme') === 'dark';
    body.setAttribute('data-theme', isDark ? 'light' : 'dark');
    localStorage.setItem('letrify_theme', isDark ? 'light' : 'dark');

    // Update toggle icon
    const icon = document.getElementById('themeIcon');
    if (icon) icon.className = isDark ? 'bi bi-moon-fill' : 'bi bi-sun-fill';
}

function initTheme() {
    const saved = localStorage.getItem('letrify_theme') || 'light';
    document.body.setAttribute('data-theme', saved);
    const icon = document.getElementById('themeIcon');
    if (icon) icon.className = saved === 'dark' ? 'bi bi-sun-fill' : 'bi bi-moon-fill';
}

// ===================== INIT =====================

document.addEventListener('DOMContentLoaded', () => {
    // Theme
    initTheme();

    // Auth forms
    const loginForm = document.getElementById('loginForm');
    if (loginForm) loginForm.addEventListener('submit', handleLogin);

    const regForm = document.getElementById('registerForm');
    if (regForm) regForm.addEventListener('submit', handleRegister);

    // Search on Enter
    const searchInput = document.getElementById('addBookSearch');
    if (searchInput) {
        searchInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') searchBooksToAdd();
        });
    }

    // Start router
    initRouter();
    console.log('✓ Letrify carregado!');
});
