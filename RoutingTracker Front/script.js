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
        </div>
    `;

    try {
        const response = await fetch(
            `${API_BASE_URL}/api/route/calculate`,
            {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    from,
                    to
                })
            }
        );

        if (!response.ok) {
            const errorData =
                await response.json().catch(() => ({}));

            throw new Error(
                errorData.error ||
                `Request failed with status ${response.status}`
            );
        }

        const data = await response.json();

        console.log('Backend response:', data);

        renderResult(data);

    } catch (err) {
        console.error(err);

        showError(err.message);

    } finally {
        btn.disabled = false;
        btn.querySelector('.btn-label').textContent =
            'Calculate route';
    }
}


function renderResult(data) {
    const resultDiv = document.getElementById('result');

    if (!data || !Array.isArray(data.routes)) {
        showError(
            'Backend returned an unexpected response format.'
        );

        console.error(
            'Unexpected backend response:',
            data
        );

        return;
    }

    if (data.routes.length === 0) {
        showError('No route data was returned.');

        return;
    }

    const groupedRoutes = {};

    for (const route of data.routes) {
        const provider =
            route.provider || 'unknown';

        if (!groupedRoutes[provider]) {
            groupedRoutes[provider] = [];
        }

        groupedRoutes[provider].push(route);
    }

    let html = `
        <div class="result-title">
            Route summary
        </div>

        <div class="result-row">
            <span class="result-label">
                From
            </span>

            <span class="result-value">
                ${escapeHtml(data.from || '-')}
            </span>
        </div>

        <div class="result-row">
            <span class="result-label">
                To
            </span>

            <span class="result-value">
                ${escapeHtml(data.to || '-')}
            </span>
        </div>
    `;

    for (const [provider, routes] of Object.entries(groupedRoutes)) {
        html += `
            <div class="provider-section">
                <div class="provider-title">
                    ${escapeHtml(provider)}
                </div>
        `;

        for (const route of routes) {
            html += renderRoute(route);
        }

        html += `
            </div>
        `;
    }

    resultDiv.innerHTML = html;
}


function renderRoute(route) {
    const transportMode =
        formatTransportMode(route.transportMode);

    const distance =
        isNumber(route.distanceKm)
            ? `${route.distanceKm.toFixed(2)} km`
            : '-';

    const duration =
        isNumber(route.durationMinutes)
            ? `${route.durationMinutes.toFixed(1)} min`
            : '-';

    const hasNoTraffic =
        isNumber(route.durationWithoutTrafficMinutes);

    const noTrafficDuration =
        hasNoTraffic
            ? `${route.durationWithoutTrafficMinutes.toFixed(1)} min`
            : '-';

    let trafficHtml = '';

    if (route.trafficAvailable) {
        trafficHtml = route.trafficUsed
            ? `
                <span class="traffic-high">
                    Available / Used
                </span>
            `
            : `
                <span class="traffic-medium">
                    Available / Not used
                </span>
            `;
    } else {
        trafficHtml = `
            <span class="result-muted">
                Not available
            </span>
        `;
    }

    let differenceHtml = '';

    if (
        hasNoTraffic &&
        isNumber(route.durationMinutes)
    ) {
        const difference =
            route.durationMinutes -
            route.durationWithoutTrafficMinutes;

        const sign =
            difference > 0 ? '+' : '';

        differenceHtml = `
            <span class="badge">
                ${sign}${difference.toFixed(1)} min
            </span>
        `;
    }

    let errorHtml = '';

    if (route.error) {
        errorHtml = `
            <div class="route-error">
                ${escapeHtml(route.error)}
            </div>
        `;
    }

    return `
        <div class="route-card">

            <div class="route-header">
                <span class="transport-title">
                    ${transportMode}
                </span>
            </div>

            <div class="result-row">
                <span class="result-label">
                    Distance
                </span>

                <span class="result-value">
                    ${distance}
                </span>
            </div>

            <div class="result-row">
                <span class="result-label">
                    Travel time
                </span>

                <span class="result-value">
                    ${duration}
                    ${differenceHtml}
                </span>
            </div>

            <div class="result-row">
                <span class="result-label">
                    Without traffic
                </span>

                <span class="result-value">
                    ${noTrafficDuration}
                </span>
            </div>

            <div class="result-row">
                <span class="result-label">
                    Traffic
                </span>

                <span class="result-value">
                    ${trafficHtml}
                </span>
            </div>

            ${errorHtml}

        </div>
    `;
}


function formatTransportMode(mode) {
    if (!mode) {
        return 'Unknown';
    }

    switch (mode.toLowerCase()) {
        case 'car':
            return '🚗 Car';

        case 'bicycle':
        case 'bike':
            return '🚲 Bicycle';

        case 'scooter':
            return '🛴 Scooter';

        default:
            return mode;
    }
}


function isNumber(value) {
    return (
        typeof value === 'number' &&
        Number.isFinite(value)
    );
}


function escapeHtml(value) {
    return String(value)
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#039;');
}


function showError(message) {
    const resultDiv =
        document.getElementById('result');

    resultDiv.classList.add('visible');

    resultDiv.innerHTML = `
        <div class="error">
            ⚠️ ${escapeHtml(message)}
        </div>
    `;
}