import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../../services/api';
import styles from './ListarClientes.module.css';
import { useAuth } from '../../context/AuthContext';
import { FaArrowLeft, FaTrash } from 'react-icons/fa';

function ListarClientes() {
    const navigate = useNavigate();
    const { user } = useAuth();
    const [clients, setClients] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const isAdmin = user?.roles.includes('Admin');

    const fetchClients = useCallback(async () => {
        try {
            setLoading(true);
            const response = await api.get('/api/users');
            setClients(response.data);
            setError(null);
            
            // DICA DE DEBUG: 
            // Se der erro de novo, descomente a linha abaixo e olhe no console do navegador (F12)
            // para ver se o "id" ou "Id" está vindo do backend.
            // console.log("Dados recebidos:", response.data);
            
        } catch (err) {
            console.error("Erro ao buscar clientes:", err);
            setError("Não foi possível carregar a lista de clientes.");
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        if (isAdmin) {
            fetchClients();
        } else {
            setError("Acesso negado. Apenas administradores.");
            setLoading(false);
        }
    }, [fetchClients, isAdmin]);

    const handleDelete = async (clientId, clientName) => {
        if (!clientId) {
            alert("Erro: ID do cliente não encontrado. Verifique o Backend.");
            return;
        }

        const confirmMessage = `ATENÇÃO: Deseja excluir/anonimizar o cliente "${clientName}"?\n\n` + 
                               `Esta ação removerá o acesso dele e apagará dados pessoais.`;

        if (window.confirm(confirmMessage)) {
            try {
                await api.delete(`/api/users/${clientId}`);
                alert('Cliente excluído com sucesso!');
                fetchClients(); 
            } catch (err) {
                console.error("Erro ao excluir cliente:", err);
                // Mostra o erro real se vier do backend
                const mensagemErro = err.response?.data?.title || "Erro ao tentar excluir o cliente.";
                alert(mensagemErro); 
            }
        }
    };
    
    const handleVoltar = () => {
        navigate(-1); 
    };

    // --- LÓGICA DO FILTRO ---
    // Filtra para remover da lista visual quem já foi excluído/anonimizado
    // Ajuste a string "Usuário Excluído" para ser IGUAL ao que você definiu no Backend (UserService)
    const activeClients = clients.filter(client => 
        client.fullName !== "Usuário Removido" && 
        !client.email.includes("deleted_") // Segurança extra pelo email
    );

    return (
        <div className={styles.page}>
            <header className={styles.header}>
                <button onClick={handleVoltar} className={styles.backButton}>
                    <FaArrowLeft /> Voltar
                </button>
                <h1>Gestão de Clientes</h1>
                <div style={{width: '100px'}}></div>
            </header>

            <main className={styles.content}>
                {loading && <p style={{color: '#ccc', textAlign: 'center'}}>Carregando...</p>}
                
                {error && <p className={styles.error}>{error}</p>}
                
                {!loading && !error && (
                    <div className={styles.cardGrid}>
                        {activeClients.length > 0 ? activeClients.map(client => {
                            // Garante que pega o ID seja maiúsculo ou minúsculo
                            const currentId = client.id || client.Id;

                            return (
                                <div key={currentId} className={styles.clientCard}>
                                    
                                    {isAdmin && (
                                        <button 
                                            className={styles.deleteButton} 
                                            onClick={() => handleDelete(currentId, client.fullName)}
                                            title="Excluir/Anonimizar Cliente"
                                        >
                                            <FaTrash />
                                        </button>
                                    )}

                                    <h3>{client.fullName}</h3>
                                    
                                    <p className={styles.infoItem}>
                                        <strong>Email:</strong> {client.email || 'N/A'}
                                    </p>
                                    <p className={styles.infoItem}>
                                        <strong>Telefone:</strong> {client.phoneNumber || 'N/A'}
                                    </p>
                                </div>
                            );
                        }) : (
                            <p style={{color: '#ccc'}}>Nenhum cliente ativo encontrado.</p>
                        )}
                    </div>
                )}
            </main>
        </div>
    );
}

export default ListarClientes;