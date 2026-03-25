import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../../services/api';
import styles from './TermosBarbeiro.module.css'; 
import { useAuth } from '../../context/AuthContext'; 

function TermosBarbeiro() {
    const navigate = useNavigate();
    const { logout } = useAuth(); 
    
    const [accepted, setAccepted] = useState(false);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!accepted) {
            setError("Você precisa aceitar os termos para continuar.");
            return;
        }
        
        setLoading(true);
        setError(null);
        
        try {
            // 1. Salva o aceite no backend
            await api.post('/api/barber/accept-terms');
            
            // 2. Redireciona para o painel
            navigate('/gestao');

        } catch (err) {
            setLoading(false);
            setError("Erro ao salvar o aceite. Tente novamente ou contate o administrador.");
            console.error(err);
        }
    };

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    return (
        <div className={styles.page}>
            <header className={styles.header}>
                <h1>Termos de Uso (Profissional)</h1>
                <button onClick={handleLogout} className={styles.logoutButton}>
                    Sair
                </button>
            </header>

            <main className={styles.content}>
                
                <p className={styles.intro}>Bem-vindo à equipe! Seu acesso ao Painel de Gestão está quase liberado. Para garantir um ambiente seguro e profissional, precisamos que você leia e concorde com as regras abaixo.</p>

                <h2>1. O seu papel no Sistema</h2>
                <p>O Painel de Gestão é sua ferramenta de trabalho. Ao utilizá-lo, você concorda em seguir as diretrizes de funcionamento da barbearia e respeitar as leis de proteção de dados.</p>

                <h2>2. Suas Responsabilidades</h2>
                <p>Para que a agenda funcione perfeitamente, você se compromete a:</p>
                <ul>
                    <li><strong>Ser Profissional:</strong> Atender os clientes com pontualidade e qualidade.</li>
                    <li><strong>Cuidar da Agenda:</strong> Manter seus horários de trabalho e pausas sempre atualizados no sistema. Lembre-se: o cliente só consegue agendar se a sua agenda estiver configurada corretamente.</li>
                    <li><strong>Segurança:</strong> Sua senha é pessoal e intransferível. Nunca compartilhe seu login com outras pessoas.</li>
                </ul>

                <h2>3. Dados dos Clientes e LGPD (Muito Importante)</h2>
                <p>Como barbeiro, você verá informações dos clientes (Nome, Serviço e Horário). Pela lei (LGPD), você é responsável por manter o sigilo desses dados.</p>

                <h3>3.1. O que você NÃO pode fazer:</h3>
                <ul>
                    <li><strong>Copiar dados:</strong> Não anote telefones ou nomes de clientes em agendas de papel ou no seu WhatsApp pessoal sem autorização.</li>
                    <li><strong>Usar para outros fins:</strong> Não use os dados para mandar propagandas ou contatar o cliente por motivos pessoais.</li>
                    <li><strong>Compartilhar:</strong> Nunca envie prints ou dados de clientes para terceiros.</li>
                </ul>
                <p><strong>Atenção:</strong> O vazamento de dados de clientes é uma infração grave e pode resultar no desligamento imediato da plataforma e em processos legais.</p>

                <h2>4. Gestão de Serviços</h2>
                <p>Ao cadastrar ou editar serviços, você deve garantir que as informações sejam reais:</p>
                <ul>
                    <li><strong>Preço:</strong> O valor deve ser o que será cobrado no final.</li>
                    <li><strong>Duração:</strong> O tempo do serviço deve ser exato. O sistema usa essa informação para calcular os encaixes na sua agenda. Se a duração estiver errada, sua agenda pode ficar bagunçada.</li>
                </ul>

                <h2>5. Aceite</h2>
                <p>Ao marcar a caixa abaixo, você confirma que entendeu suas responsabilidades profissionais e o compromisso com a privacidade dos dados dos clientes.</p>
                
                <form onSubmit={handleSubmit} className={styles.formAceite}>
                    <div className={styles.termsGroup}>
                        <input
                            type="checkbox"
                            id="terms"
                            checked={accepted}
                            onChange={(e) => setAccepted(e.target.checked)}
                        />
                        <label htmlFor="terms">
                            Li e aceito os Termos de Uso Profissional.
                        </label>
                    </div>

                    {error && <p className={styles.error}>{error}</p>}

                    <button 
                        type="submit" 
                        className={styles.submitButton}
                        disabled={!accepted || loading}
                    >
                        {loading ? 'Salvando...' : 'Aceitar e Acessar Painel'}
                    </button>
                </form>
            </main>
        </div>
    );
}

export default TermosBarbeiro;