// Configuración de APIs
const API_CONFIG = {
    central: 'http://localhost:5000/api',
    inventario: 'http://localhost:5001/api',
    facturacion: 'http://localhost:5098/api'
};

// Estado global de la aplicación
const appState = {
    currentSection: 'dashboard',
    agricultores: [],
    cosechas: [],
    insumos: [],
    facturas: [],
    currentModal: null,
    editingItem: null
};

// Inicialización de la aplicación
document.addEventListener('DOMContentLoaded', function() {
    initializeApp();
});

async function initializeApp() {
    setupNavigation();
    setupEventListeners();
    await loadDashboardData();
    showSection('dashboard');
}

// Configuración de navegación
function setupNavigation() {
    const navLinks = document.querySelectorAll('.nav-link');
    navLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            const section = this.getAttribute('data-section');
            showSection(section);
        });
    });
}

// Configuración de event listeners
function setupEventListeners() {
    // Formularios
    document.getElementById('agricultor-form').addEventListener('submit', handleAgricultorSubmit);
    document.getElementById('cosecha-form').addEventListener('submit', handleCosechaSubmit);
    document.getElementById('insumo-form').addEventListener('submit', handleInsumoSubmit);
    
    // Modal overlay
    document.getElementById('modal-overlay').addEventListener('click', function(e) {
        if (e.target === this) {
            closeModal();
        }
    });
}

// Navegación entre secciones
function showSection(sectionName) {
    // Actualizar navegación
    document.querySelectorAll('.nav-link').forEach(link => {
        link.classList.remove('active');
    });
    document.querySelector(`[data-section="${sectionName}"]`).classList.add('active');
    
    // Mostrar sección
    document.querySelectorAll('.section').forEach(section => {
        section.classList.remove('active');
    });
    document.getElementById(sectionName).classList.add('active');
    
    appState.currentSection = sectionName;
    
    // Cargar datos específicos de la sección
    switch(sectionName) {
        case 'dashboard':
            loadDashboardData();
            break;
        case 'agricultores':
            loadAgricultores();
            break;
        case 'cosechas':
            loadCosechas();
            break;
        case 'inventario':
            loadInventario();
            break;
        case 'facturas':
            loadFacturas();
            break;
    }
}

// Funciones de API
async function apiRequest(url, options = {}) {
    try {
        showLoading();
        const response = await fetch(url, {
            headers: {
                'Content-Type': 'application/json',
                ...options.headers
            },
            ...options
        });
        
        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.message || `Error ${response.status}: ${response.statusText}`);
        }
        
        return await response.json();
    } catch (error) {
        console.error('API Error:', error);
        showToast(error.message || 'Error de conexión', 'error');
        throw error;
    } finally {
        hideLoading();
    }
}

// Dashboard
async function loadDashboardData() {
    try {
        const [agricultores, cosechas, insumos, facturas, estadisticasInventario] = await Promise.all([
            apiRequest(`${API_CONFIG.central}/agricultoras`),
            apiRequest(`${API_CONFIG.central}/cosechas`),
            apiRequest(`${API_CONFIG.inventario}/insumos`),
            apiRequest(`${API_CONFIG.facturacion}/facturas`),
            apiRequest(`${API_CONFIG.inventario}/insumos/estadisticas`)
        ]);
        
        // Actualizar contadores
        document.getElementById('total-agricultores').textContent = agricultores.length;
        document.getElementById('total-cosechas').textContent = cosechas.length;
        document.getElementById('total-insumos').textContent = estadisticasInventario.totalInsumos;
        document.getElementById('total-facturas').textContent = facturas.length;
        
        // Mostrar insumos con stock bajo
        await loadStockBajo();
        
        // Mostrar cosechas recientes
        await loadCosechasRecientes(cosechas);
        
    } catch (error) {
        console.error('Error loading dashboard:', error);
    }
}

async function loadStockBajo() {
    try {
        const stockBajo = await apiRequest(`${API_CONFIG.inventario}/insumos/stock-bajo`);
        const container = document.getElementById('stock-bajo-list');
        
        if (stockBajo.length === 0) {
            container.innerHTML = '<p class="text-center">✅ Todos los insumos tienen stock adecuado</p>';
            return;
        }
        
        container.innerHTML = stockBajo.map(insumo => `
            <div class="p-2 mb-1" style="border-left: 3px solid #dc3545; background: #fff5f5;">
                <strong>${insumo.nombreInsumo}</strong><br>
                <small>Stock: ${insumo.stock} ${insumo.unidadMedida}</small>
            </div>
        `).join('');
    } catch (error) {
        document.getElementById('stock-bajo-list').innerHTML = '<p class="text-center">Error al cargar datos</p>';
    }
}

async function loadCosechasRecientes(cosechas = null) {
    try {
        if (!cosechas) {
            cosechas = await apiRequest(`${API_CONFIG.central}/cosechas`);
        }
        
        const recientes = cosechas
            .sort((a, b) => new Date(b.creadoEn) - new Date(a.creadoEn))
            .slice(0, 5);
        
        const container = document.getElementById('cosechas-recientes-list');
        
        if (recientes.length === 0) {
            container.innerHTML = '<p class="text-center">No hay cosechas registradas</p>';
            return;
        }
        
        container.innerHTML = recientes.map(cosecha => `
            <div class="p-2 mb-1" style="border-left: 3px solid #4a7c59; background: #f8fff8;">
                <strong>${cosecha.producto}</strong> - ${cosecha.toneladas} ton<br>
                <small>Agricultor: ${cosecha.agricultor?.nombre || 'Desconocido'}</small><br>
                <small>Estado: <span class="status-badge status-${cosecha.estado.toLowerCase()}">${cosecha.estado}</span></small>
            </div>
        `).join('');
    } catch (error) {
        document.getElementById('cosechas-recientes-list').innerHTML = '<p class="text-center">Error al cargar datos</p>';
    }
}

// Agricultores
async function loadAgricultores() {
    try {
        const agricultores = await apiRequest(`${API_CONFIG.central}/agricultoras`);
        appState.agricultores = agricultores;
        renderAgricultoresTable(agricultores);
    } catch (error) {
        document.getElementById('agricultores-table').innerHTML = 
            '<tr><td colspan="6" class="text-center">Error al cargar agricultores</td></tr>';
    }
}

function renderAgricultoresTable(agricultores) {
    const tbody = document.getElementById('agricultores-table');
    
    if (agricultores.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center">No hay agricultores registrados</td></tr>';
        return;
    }
    
    tbody.innerHTML = agricultores.map(agricultor => `
        <tr>
            <td>${agricultor.nombre}</td>
            <td>${agricultor.finca}</td>
            <td>${agricultor.ubicacion}</td>
            <td>${agricultor.correo}</td>
            <td>${formatDate(agricultor.fechaRegistro)}</td>
            <td>
                <button class="btn btn-sm btn-warning" onclick="editAgricultor('${agricultor.agricultorId}')">
                    <i class="fas fa-edit"></i>
                </button>
                <button class="btn btn-sm btn-danger" onclick="deleteAgricultor('${agricultor.agricultorId}')">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        </tr>
    `).join('');
}

function showAddAgricultorModal() {
    appState.editingItem = null;
    document.getElementById('agricultor-modal-title').textContent = 'Nuevo Agricultor';
    document.getElementById('agricultor-form').reset();
    showModal('agricultor-modal');
}

function editAgricultor(id) {
    const agricultor = appState.agricultores.find(a => a.agricultorId === id);
    if (!agricultor) return;
    
    appState.editingItem = agricultor;
    document.getElementById('agricultor-modal-title').textContent = 'Editar Agricultor';
    
    document.getElementById('agricultor-nombre').value = agricultor.nombre;
    document.getElementById('agricultor-finca').value = agricultor.finca;
    document.getElementById('agricultor-ubicacion').value = agricultor.ubicacion;
    document.getElementById('agricultor-correo').value = agricultor.correo;
    
    showModal('agricultor-modal');
}

async function handleAgricultorSubmit(e) {
    e.preventDefault();
    
    const formData = new FormData(e.target);
    const data = Object.fromEntries(formData);
    
    try {
        if (appState.editingItem) {
            // Actualizar
            await apiRequest(`${API_CONFIG.central}/agricultoras/${appState.editingItem.agricultorId}`, {
                method: 'PUT',
                body: JSON.stringify({
                    ...data,
                    agricultorId: appState.editingItem.agricultorId
                })
            });
            showToast('Agricultor actualizado exitosamente', 'success');
        } else {
            // Crear
            await apiRequest(`${API_CONFIG.central}/agricultoras`, {
                method: 'POST',
                body: JSON.stringify(data)
            });
            showToast('Agricultor creado exitosamente', 'success');
        }
        
        closeModal();
        loadAgricultores();
    } catch (error) {
        console.error('Error saving agricultor:', error);
    }
}

async function deleteAgricultor(id) {
    if (!confirm('¿Está seguro de que desea eliminar este agricultor?')) return;
    
    try {
        await apiRequest(`${API_CONFIG.central}/agricultoras/${id}`, {
            method: 'DELETE'
        });
        showToast('Agricultor eliminado exitosamente', 'success');
        loadAgricultores();
    } catch (error) {
        console.error('Error deleting agricultor:', error);
    }
}

// Cosechas
async function loadCosechas() {
    try {
        const cosechas = await apiRequest(`${API_CONFIG.central}/cosechas`);
        appState.cosechas = cosechas;
        renderCosechasTable(cosechas);
    } catch (error) {
        document.getElementById('cosechas-table').innerHTML = 
            '<tr><td colspan="6" class="text-center">Error al cargar cosechas</td></tr>';
    }
}

function renderCosechasTable(cosechas) {
    const tbody = document.getElementById('cosechas-table');
    
    if (cosechas.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center">No hay cosechas registradas</td></tr>';
        return;
    }
    
    tbody.innerHTML = cosechas.map(cosecha => `
        <tr>
            <td>${cosecha.agricultor?.nombre || 'Desconocido'}</td>
            <td>${cosecha.producto}</td>
            <td>${cosecha.toneladas} ton</td>
            <td>
                <span class="status-badge status-${cosecha.estado.toLowerCase()}">
                    ${cosecha.estado}
                </span>
            </td>
            <td>${formatDate(cosecha.creadoEn)}</td>
            <td>
                <button class="btn btn-sm btn-warning" onclick="cambiarEstadoCosecha('${cosecha.cosechaId}', '${cosecha.estado}')">
                    <i class="fas fa-exchange-alt"></i> Estado
                </button>
                ${cosecha.estado === 'REGISTRADA' ? `
                    <button class="btn btn-sm btn-danger" onclick="deleteCosecha('${cosecha.cosechaId}')">
                        <i class="fas fa-trash"></i>
                    </button>
                ` : ''}
            </td>
        </tr>
    `).join('');
}

async function showAddCosechaModal() {
    // Cargar agricultores para el select
    try {
        if (appState.agricultores.length === 0) {
            const agricultores = await apiRequest(`${API_CONFIG.central}/agricultoras`);
            appState.agricultores = agricultores;
        }
        
        const select = document.getElementById('cosecha-agricultor');
        select.innerHTML = '<option value="">Seleccionar agricultor...</option>' +
            appState.agricultores.map(a => `<option value="${a.agricultorId}">${a.nombre}</option>`).join('');
        
        document.getElementById('cosecha-form').reset();
        showModal('cosecha-modal');
    } catch (error) {
        showToast('Error al cargar agricultores', 'error');
    }
}

async function handleCosechaSubmit(e) {
    e.preventDefault();
    
    const formData = new FormData(e.target);
    const data = Object.fromEntries(formData);
    
    try {
        await apiRequest(`${API_CONFIG.central}/cosechas`, {
            method: 'POST',
            body: JSON.stringify({
                ...data,
                toneladas: parseFloat(data.toneladas)
            })
        });
        
        showToast('Cosecha creada exitosamente', 'success');
        closeModal();
        loadCosechas();
    } catch (error) {
        console.error('Error saving cosecha:', error);
    }
}

async function cambiarEstadoCosecha(id, estadoActual) {
    const estados = ['REGISTRADA', 'EN_PROCESO', 'FACTURADA', 'COMPLETADA'];
    const currentIndex = estados.indexOf(estadoActual);
    const nextIndex = (currentIndex + 1) % estados.length;
    const nuevoEstado = estados[nextIndex];
    
    if (!confirm(`¿Cambiar estado de ${estadoActual} a ${nuevoEstado}?`)) return;
    
    try {
        await apiRequest(`${API_CONFIG.central}/cosechas/${id}/estado`, {
            method: 'PUT',
            body: JSON.stringify(nuevoEstado)
        });
        
        showToast(`Estado actualizado a ${nuevoEstado}`, 'success');
        loadCosechas();
    } catch (error) {
        console.error('Error updating estado:', error);
    }
}

async function deleteCosecha(id) {
    if (!confirm('¿Está seguro de que desea eliminar esta cosecha?')) return;
    
    try {
        await apiRequest(`${API_CONFIG.central}/cosechas/${id}`, {
            method: 'DELETE'
        });
        showToast('Cosecha eliminada exitosamente', 'success');
        loadCosechas();
    } catch (error) {
        console.error('Error deleting cosecha:', error);
    }
}

// Inventario
async function loadInventario() {
    try {
        const insumos = await apiRequest(`${API_CONFIG.inventario}/insumos`);
        appState.insumos = insumos;
        renderInventarioTable(insumos);
    } catch (error) {
        document.getElementById('inventario-table').innerHTML = 
            '<tr><td colspan="6" class="text-center">Error al cargar inventario</td></tr>';
    }
}

function renderInventarioTable(insumos) {
    const tbody = document.getElementById('inventario-table');
    
    if (insumos.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center">No hay insumos registrados</td></tr>';
        return;
    }
    
    tbody.innerHTML = insumos.map(insumo => `
        <tr>
            <td>${insumo.nombreInsumo}</td>
            <td>
                <span class="${insumo.stock <= 10 ? 'text-danger' : ''}" style="font-weight: ${insumo.stock <= 10 ? 'bold' : 'normal'}">
                    ${insumo.stock}
                </span>
            </td>
            <td>${insumo.unidadMedida}</td>
            <td>${insumo.categoria || 'Sin categoría'}</td>
            <td>${formatDate(insumo.ultimaActualizacion)}</td>
            <td>
                <button class="btn btn-sm btn-primary" onclick="ajustarStock('${insumo.insumoId}', ${insumo.stock})">
                    <i class="fas fa-edit"></i> Stock
                </button>
                <button class="btn btn-sm btn-danger" onclick="deleteInsumo('${insumo.insumoId}')">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        </tr>
    `).join('');
}

function showAddInsumoModal() {
    document.getElementById('insumo-form').reset();
    showModal('insumo-modal');
}

async function handleInsumoSubmit(e) {
    e.preventDefault();
    
    const formData = new FormData(e.target);
    const data = Object.fromEntries(formData);
    
    try {
        await apiRequest(`${API_CONFIG.inventario}/insumos`, {
            method: 'POST',
            body: JSON.stringify({
                ...data,
                stock: parseInt(data.stock)
            })
        });
        
        showToast('Insumo creado exitosamente', 'success');
        closeModal();
        loadInventario();
    } catch (error) {
        console.error('Error saving insumo:', error);
    }
}

async function ajustarStock(id, stockActual) {
    const nuevoStock = prompt(`Stock actual: ${stockActual}\nIngrese el nuevo stock:`, stockActual);
    if (nuevoStock === null || nuevoStock === '') return;
    
    const stock = parseInt(nuevoStock);
    if (isNaN(stock) || stock < 0) {
        showToast('Stock debe ser un número válido mayor o igual a 0', 'error');
        return;
    }
    
    try {
        await apiRequest(`${API_CONFIG.inventario}/insumos/${id}/stock`, {
            method: 'PUT',
            body: JSON.stringify(stock)
        });
        
        showToast('Stock actualizado exitosamente', 'success');
        loadInventario();
    } catch (error) {
        console.error('Error updating stock:', error);
    }
}

async function deleteInsumo(id) {
    if (!confirm('¿Está seguro de que desea eliminar este insumo?')) return;
    
    try {
        await apiRequest(`${API_CONFIG.inventario}/insumos/${id}`, {
            method: 'DELETE'
        });
        showToast('Insumo eliminado exitosamente', 'success');
        loadInventario();
    } catch (error) {
        console.error('Error deleting insumo:', error);
    }
}

// Facturas
async function loadFacturas() {
    try {
        const facturas = await apiRequest(`${API_CONFIG.facturacion}/facturas`);
        appState.facturas = facturas;
        renderFacturasTable(facturas);
    } catch (error) {
        document.getElementById('facturas-table').innerHTML = 
            '<tr><td colspan="7" class="text-center">Error al cargar facturas</td></tr>';
    }
}

function renderFacturasTable(facturas) {
    const tbody = document.getElementById('facturas-table');
    
    if (facturas.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" class="text-center">No hay facturas generadas</td></tr>';
        return;
    }
    
    tbody.innerHTML = facturas.map(factura => `
        <tr>
            <td><small>${factura.facturaId.substring(0, 8)}...</small></td>
            <td>${factura.nombreAgricultor}</td>
            <td>${factura.producto}</td>
            <td>$${formatNumber(factura.total)}</td>
            <td>
                <span class="status-badge status-${factura.estado.toLowerCase()}">
                    ${factura.estado}
                </span>
            </td>
            <td>${formatDate(factura.fechaCreacion)}</td>
            <td>
                ${factura.estado === 'PENDIENTE' ? `
                    <button class="btn btn-sm btn-success" onclick="marcarComoPagada('${factura.facturaId}')">
                        <i class="fas fa-check"></i> Pagar
                    </button>
                ` : ''}
                <button class="btn btn-sm btn-primary" onclick="verDetalleFactura('${factura.facturaId}')">
                    <i class="fas fa-eye"></i> Ver
                </button>
            </td>
        </tr>
    `).join('');
}

async function marcarComoPagada(id) {
    if (!confirm('¿Marcar esta factura como pagada?')) return;
    
    try {
        await apiRequest(`${API_CONFIG.facturacion}/facturas/${id}/estado`, {
            method: 'PUT',
            body: JSON.stringify({
                estado: 'PAGADA',
                fechaPago: new Date().toISOString()
            })
        });
        
        showToast('Factura marcada como pagada', 'success');
        loadFacturas();
    } catch (error) {
        console.error('Error updating factura:', error);
    }
}

async function verDetalleFactura(id) {
    try {
        const factura = await apiRequest(`${API_CONFIG.facturacion}/facturas/${id}`);
        
        const detalles = factura.detalles.map(detalle => `
            <tr>
                <td>${detalle.concepto}</td>
                <td>${detalle.cantidad} ${detalle.unidadMedida}</td>
                <td>$${formatNumber(detalle.precioUnitario)}</td>
                <td>$${formatNumber(detalle.subtotal)}</td>
            </tr>
        `).join('');
        
        const modalContent = `
            <div class="modal-header">
                <h3>Detalle de Factura</h3>
                <button class="modal-close" onclick="closeModal()">&times;</button>
            </div>
            <div class="modal-body">
                <div class="mb-2">
                    <strong>Agricultor:</strong> ${factura.nombreAgricultor}<br>
                    <strong>Producto:</strong> ${factura.producto}<br>
                    <strong>Fecha:</strong> ${formatDate(factura.fechaCreacion)}<br>
                    <strong>Estado:</strong> <span class="status-badge status-${factura.estado.toLowerCase()}">${factura.estado}</span>
                </div>
                
                <table class="table">
                    <thead>
                        <tr>
                            <th>Concepto</th>
                            <th>Cantidad</th>
                            <th>Precio Unit.</th>
                            <th>Subtotal</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${detalles}
                    </tbody>
                </table>
                
                <div class="text-right mt-2">
                    <strong>Subtotal: $${formatNumber(factura.subtotal)}</strong><br>
                    <strong>Impuesto (${factura.porcentajeImpuesto}%): $${formatNumber(factura.montoImpuesto)}</strong><br>
                    <strong>Total: $${formatNumber(factura.total)}</strong>
                </div>
            </div>
        `;
        
        document.querySelector('#modal-overlay .modal').innerHTML = modalContent;
        showModal();
    } catch (error) {
        showToast('Error al cargar detalle de factura', 'error');
    }
}

// Funciones de utilidad
function showModal(modalId = null) {
    const overlay = document.getElementById('modal-overlay');
    if (modalId) {
        // Ocultar todos los modales
        overlay.querySelectorAll('.modal').forEach(modal => modal.style.display = 'none');
        // Mostrar el modal específico
        document.getElementById(modalId).style.display = 'block';
    }
    overlay.classList.add('active');
    appState.currentModal = modalId;
}

function closeModal() {
    document.getElementById('modal-overlay').classList.remove('active');
    appState.currentModal = null;
    appState.editingItem = null;
}

function showLoading() {
    document.getElementById('loading-spinner').classList.add('active');
}

function hideLoading() {
    document.getElementById('loading-spinner').classList.remove('active');
}

function showToast(message, type = 'info') {
    const container = document.getElementById('toast-container');
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    
    const icon = type === 'success' ? 'check-circle' : 
                 type === 'error' ? 'exclamation-circle' :
                 type === 'warning' ? 'exclamation-triangle' : 'info-circle';
    
    toast.innerHTML = `
        <i class="fas fa-${icon}"></i>
        <span>${message}</span>
    `;
    
    container.appendChild(toast);
    
    setTimeout(() => {
        toast.remove();
    }, 5000);
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('es-ES', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function formatNumber(number) {
    return new Intl.NumberFormat('es-ES').format(number);
}

// Manejo de errores global
window.addEventListener('error', function(e) {
    console.error('Global error:', e.error);
    showToast('Ha ocurrido un error inesperado', 'error');
});

window.addEventListener('unhandledrejection', function(e) {
    console.error('Unhandled promise rejection:', e.reason);
    showToast('Error de conexión con el servidor', 'error');
});
