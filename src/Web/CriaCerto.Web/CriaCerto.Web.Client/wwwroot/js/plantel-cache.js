window.plantelCache = (() => {
  const dbName = 'criacerto-plantel';
  const dbVersion = 2;
  const sowStore = 'sows';
  const pendingStore = 'pendingOps';

  function openDb() {
    return new Promise((resolve, reject) => {
      const request = indexedDB.open(dbName, dbVersion);
      request.onupgradeneeded = () => {
        const db = request.result;
        if (!db.objectStoreNames.contains(sowStore)) {
          const store = db.createObjectStore(sowStore, { keyPath: 'id' });
          store.createIndex('tagId', 'tagId', { unique: false });
          store.createIndex('reproductiveStatus', 'reproductiveStatus', { unique: false });
        }
        if (!db.objectStoreNames.contains(pendingStore)) {
          const pending = db.createObjectStore(pendingStore, { keyPath: 'id' });
          pending.createIndex('type', 'type', { unique: false });
          pending.createIndex('createdAt', 'createdAt', { unique: false });
        }
      };
      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error);
    });
  }

  async function saveSows(sows) {
    const db = await openDb();
    const tx = db.transaction(sowStore, 'readwrite');
    const store = tx.objectStore(sowStore);
    store.clear();
    for (const sow of sows || []) {
      store.put({ ...sow, cachedAt: new Date().toISOString() });
    }
    await waitForTransaction(tx);
    db.close();
  }

  async function getSows(search, status) {
    const db = await openDb();
    const tx = db.transaction(sowStore, 'readonly');
    const request = tx.objectStore(sowStore).getAll();
    const rows = await waitForRequest(request);
    db.close();

    const query = (search || '').trim().toLowerCase();
    return rows.filter((row) => {
      const matchesSearch = !query ||
        (row.tagId || '').toLowerCase().includes(query) ||
        (row.nickname || '').toLowerCase().includes(query);
      const matchesStatus = !status || String(row.reproductiveStatus) === String(status);
      return matchesSearch && matchesStatus;
    });
  }

  async function enqueueOp(type, payload) {
    const db = await openDb();
    const tx = db.transaction(pendingStore, 'readwrite');
    tx.objectStore(pendingStore).add({
      id: crypto.randomUUID(),
      type,
      payload,
      createdAt: new Date().toISOString()
    });
    await waitForTransaction(tx);
    db.close();
  }

  async function getPendingOps() {
    const db = await openDb();
    const tx = db.transaction(pendingStore, 'readonly');
    const request = tx.objectStore(pendingStore).getAll();
    const rows = await waitForRequest(request);
    db.close();
    return rows.sort((a, b) => (a.createdAt || '').localeCompare(b.createdAt || ''));
  }

  async function getPendingOpsCount() {
    const rows = await getPendingOps();
    return rows.length;
  }

  async function removeOp(id) {
    const db = await openDb();
    const tx = db.transaction(pendingStore, 'readwrite');
    tx.objectStore(pendingStore).delete(id);
    await waitForTransaction(tx);
    db.close();
  }

  function isOnline() {
    return typeof navigator !== 'undefined' ? navigator.onLine : true;
  }

  function waitForRequest(request) {
    return new Promise((resolve, reject) => {
      request.onsuccess = () => resolve(request.result || []);
      request.onerror = () => reject(request.error);
    });
  }

  function waitForTransaction(tx) {
    return new Promise((resolve, reject) => {
      tx.oncomplete = () => resolve();
      tx.onerror = () => reject(tx.error);
      tx.onabort = () => reject(tx.error);
    });
  }

  return {
    saveSows,
    getSows,
    enqueueOp,
    getPendingOps,
    getPendingOpsCount,
    removeOp,
    isOnline
  };
})();
