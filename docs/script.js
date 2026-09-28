const API_BASE_URL = 'http://localhost:5261';

async function calculateRoute() {
    const from = document.getElementById('from').value.trim();
    const to = document.getElementById('to').value.trim();
    const resultDiv = document.getElementById('result');
    const btn = document.getElementById('calculateBtn');

    if (!from || !to) {
        showError('Please fill in both fields.');
        return;
    }

    btn.disabled = true;
    btn.querySelector('.btn-label').textContent = 'Calculating...';

    resultDiv.classList.add('visible');
    resultDiv.innerHTML = `
        <div class="loading">
            <span class="spinner"></span>
            Fetching route data...
        </div>`;

    try {
        const response = await fetch(`${API_BASE_URL}/api/route/calculate`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ from, to })
        });

        const data = await response.json().catch(() => ({}));

        if (!response.ok) {
            throw new Error(
                data.error || `Request failed with status ${response.status}`
            );
        }

        renderResult(data);
    } catch (error) {
        showError(error.message);
    } finally {
        btn.disabled = false;
        btn.querySelector('.btn-label').textContent = 'Calculate route';
    }
}

function renderResult(data) {
    const resultDiv = document.getElementById('result');
    const providerRoutes = Array.isArray(data.routes) ? data.routes : [];

    if (providerRoutes.length === 0) {
        showError('No route data was returned by the server.');
        return;
    }

    resultDiv.innerHTML = `
        <div class="result-title">Route results</div>
        <div class="route-meta">
            <div><strong>From:</strong> ${escapeHtml(data.from)}</div>
            <div><strong>To:</strong> ${escapeHtml(data.to)}</div>
        </div>
        <div class="providers">
            ${providerRoutes.map(renderProvider).join('')}
        </div>`;
}

function renderProvider(provider) {
    const providerName = formatProviderName(provider.provider);
    const modeName = formatModeName(provider.transportMode);
    const routes = Array.isArray(provider.routes) ? provider.routes : [];

    if (routes.length === 0) {
        return `
            <section class="provider-card error-provider">
                <div class="provider-header">
                    <div>
                        <div class="provider-name">${escapeHtml(providerName)}</div>
                        <div class="mode-name">${escapeHtml(modeName)}</div>
                    </div>
                    <span class="route-count">No routes</span>
                </div>
                <div class="provider-error">${escapeHtml(provider.error || 'Route not found')}</div>
            </section>`;
    }

    return `
        <section class="provider-card">
            <div class="provider-header">
                <div>
                    <div class="provider-name">${escapeHtml(providerName)}</div>
                    <div class="mode-name">${escapeHtml(modeName)}</div>
                </div>
                <span class="route-count">${routes.length} ${routes.length === 1 ? 'route' : 'routes'}</span>
            </div>

            <div class="traffic-info">
                Traffic: ${provider.trafficUsed ? 'Used' : provider.trafficAvailable ? 'Available' : 'Not available'}
            </div>

            <div class="route-list">
                ${routes.map(renderRoute).join('')}
            </div>
        </section>`;
}

function renderRoute(route) {
    return `
        <div class="route-option">
            <div class="route-option-header">
                <span class="route-number">Route ${route.routeNumber}</span>
                ${route.algorithm ? `<span class="algorithm">${escapeHtml(route.algorithm)}</span>` : ''}
            </div>

            <div class="route-stats">
                <div class="stat">
                    <span class="stat-label">Distance</span>
                    <span class="stat-value">${formatNumber(route.distanceKm)} km</span>
                </div>
                <div class="stat">
                    <span class="stat-label">Time</span>
                    <span class="stat-value">${formatNumber(route.durationMinutes)} min</span>
                </div>
            </div>
        </div>`;
}

function formatProviderName(name) {
    if (!name) return 'Unknown provider';
    return name.toLowerCase() === '2gis' ? '2GIS' :
           name.toLowerCase() === 'yandex' ? 'Yandex' : name;
}

function formatModeName(mode) {
    const names = {
        car: '🚗 Car',
        bicycle: '🚲 Bicycle',
        scooter: '🛴 Scooter'
    };

    return names[(mode || '').toLowerCase()] || mode || 'Unknown mode';
}

function formatNumber(value) {
    const number = Number(value);
    return Number.isFinite(number) ? number.toFixed(2) : '—';
}

function escapeHtml(value) {
    return String(value ?? '')
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#039;');
}

function showError(message) {
    const resultDiv = document.getElementById('result');
    resultDiv.classList.add('visible');
    resultDiv.innerHTML = `<div class="error">⚠️ ${escapeHtml(message)}</div>`;
}
