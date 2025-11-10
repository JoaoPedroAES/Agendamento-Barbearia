import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../../services/api';
import styles from './TermosBarbeiro.module.css'; 
import { useAuth } from '../../context/AuthContext'; // Importa o AuthContext

function TermosBarbeiro() {
    const navigate = useNavigate();
    // A função 'updateUserTerms' não é mais necessária aqui, 
    // pois o 'ManagementDashboard' fará a verificação novamente após o aceite.
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
            // 1. Chama a API que criamos no backend para salvar o aceite
            await api.post('/api/barber/accept-terms');
            
            // 2. Redireciona para o painel de gestão
            // O 'useEffect' lá vai rodar de novo, verá 'true' e deixará o usuário entrar.
            navigate('/gestao');

        } catch (err) {
            setLoading(false);
            setError("Erro ao salvar o aceite. Tente novamente ou contate o administrador.");
            console.error(err);
        }
    };

    // Função de Logout (caso o barbeiro não queira aceitar e queira sair)
    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    return (
        <div className={styles.page}>
            <header className={styles.header}>
                <h1>Termos de Uso e Responsabilidade (Profissional)</h1>
                {/* O botão 'Voltar' é substituído por 'Sair' neste fluxo obrigatório */}
                <button onClick={handleLogout} className={styles.logoutButton}>
                    Sair
                </button>
            </header>

            <main className={styles.content}>
                
                <p className={styles.intro}>Bem-vindo à equipe! Seu acesso ao Painel de Gestão está quase liberado. Antes de continuar, por favor, leia e aceite os termos de uso profissional e a política de tratamento de dados.</p>

                <h2>1. Objeto</h2>
                <p>Estes Termos regem o uso do Painel de Gestão ("Plataforma") por você ("Barbeiro"), estabelecendo suas obrigações, responsabilidades e as diretrizes de tratamento de dados de clientes, em conformidade com a Lei Geral de Proteção de Dados (LGPD).</p>

                <h2>2. Responsabilidades do Barbeiro</h2>
                <p>Ao aceitar estes termos, você se compromete a:</p>
                <ul>
                    <li>**Profissionalismo:** Prestar os serviços agendados com pontualidade, qualidade e profissionalismo.</li>
                    <li>**Gestão de Agenda:** Manter seus horários de trabalho (`WorkSchedule`) e pausas rigorosamente atualizados na Plataforma. A disponibilidade de horários para os clientes depende exclusivamente desta configuração.</li>
                    <li>**Uso da Plataforma:** Utilizar o painel de gestão apenas para fins profissionais relacionados à barbearia, não compartilhando seu acesso (`login` e `senha`) com terceiros.</li>
                </ul>

                <h2>3. Política de Privacidade e Tratamento de Dados (LGPD)</h2>
                <p>Como Barbeiro, você terá acesso a Dados Pessoais de Clientes. Você é considerado um **Agente de Tratamento** e deve seguir rigorosamente a LGPD.</p>

                <h3>3.1. Dados Acessíveis</h3>
                <p>Você terá acesso aos seguintes dados dos clientes que agendarem com você:</p>
                <ul>
                    <li>Nome Completo do Cliente.</li>
                    <li>Serviços selecionados.</li>
                    <li>Data e Hora do agendamento.</li>
                </ul>
                <p>Você **não** terá acesso à senha, endereço ou dados de pagamento dos clientes.</p>

                <h3>3.2. Finalidade e Confidencialidade (Obrigatório)</h3>
                <p>Você se compromete a utilizar os dados dos clientes **única e exclusivamente** para a finalidade de prestar o serviço agendado. É estritamente proibido:</p>
                <ul>
                    <li>Copiar, salvar ou armazenar dados de clientes fora da plataforma (ex: em agendas pessoais, WhatsApp pessoal, etc.).</li>
                    <li>Utilizar os dados dos clientes para qualquer outra finalidade (ex: marketing pessoal não autorizado, contato por motivos não relacionados ao agendamento).</li>
                    <li>Compartilhar os dados dos clientes com qualquer terceiro.</li>
                </ul>
                <p>O descumprimento desta cláusula constitui violação grave da LGPD e destes Termos, sujeitando o Barbeiro ao desligamento imediato da plataforma e às sanções legais cabíveis.</p>

                <h2>4. Gestão de Serviços (CRUD)</h2>
                <p>O Barbeiro (ou Admin) é responsável por manter a lista de serviços (`Gerenciar Serviços`) atualizada. Você concorda em cadastrar informações fidedignas, incluindo o `Nome` do serviço, o `Preço` (em R$) e a `Duração (minutos)` correta. A `Duração` é o fator principal que o sistema utiliza para calcular a disponibilidade da agenda (RN-AGENDA-004).</p>

                <h2>5. Aceite</h2>
                <p>Ao marcar a caixa abaixo e clicar em "Aceitar e Continuar", você declara que leu, compreendeu e concorda integralmente com todas as cláusulas destes Termos de Uso e Responsabilidade Profissional.</p>
                
                <form onSubmit={handleSubmit} className={styles.formAceite}>
                    <div className={styles.termsGroup}>
                        <input
                            type="checkbox"
                            id="terms"
                            checked={accepted}
                            onChange={(e) => setAccepted(e.target.checked)}
                        />
                        <label htmlFor="terms">
                            Eu li, compreendi e aceito os Termos de Uso do Profissional.
                        </label>
                    </div>

                    {error && <p className={styles.error}>{error}</p>}

                    <button 
                        type="submit" 
                        className={styles.submitButton}
                        disabled={!accepted || loading}
                    >
                        {loading ? 'Salvando...' : 'Aceitar e Continuar'}
                    </button>
                </form>
            </main>
        </div>
    );
}

export default TermosBarbeiro;