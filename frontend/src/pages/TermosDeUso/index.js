import React from 'react';
import { useNavigate } from 'react-router-dom';
import styles from './TermosDeUso.module.css';
import { FaArrowLeft } from 'react-icons/fa';

function TermosDeUso() {
    const navigate = useNavigate();

    // Função para voltar para a página anterior (ex: cadastro ou login)
    const handleVoltar = () => {
        navigate(-1);
    };

    return (
        <div className={styles.page}>
            {/* --- Cabeçalho Padrão --- */}
            <header className={styles.header}>
                <button onClick={handleVoltar} className={styles.backButton}>
                    <FaArrowLeft /> Voltar
                </button>
                <h1>Termos de Uso e Política de Privacidade</h1>
                <div style={{width: '100px'}}></div> {/* Espaçador */}
            </header>

            <main className={styles.content}>
                
                <p className={styles.lastUpdate}>Última atualização: 09 de Novembro de 2025</p>

                <p>Estes Termos de Uso e Política de Privacidade ("Termos") regem o acesso e a utilização da plataforma de agendamento de barbearia ("Plataforma"). Ao criar uma conta e marcar a caixa "Eu li e aceito os Termos de Uso", você ("Usuário") expressa seu consentimento livre, informado e inequívoco com todas as cláusulas aqui presentes.</p>

                <h2>1. Dos Serviços Prestados</h2>
                <p>A Plataforma atua como uma intermediária digital, fornecendo uma ferramenta para que Usuários (`Clientes`) possam visualizar horários disponíveis, selecionar serviços e realizar agendamentos com os profissionais (`Barbeiros`) cadastrados.</p>

                <h2>2. Das Obrigações do Usuário (Cliente)</h2>
                <p>Ao utilizar a Plataforma, o Usuário concorda em:</p>
                <ul>
                    <li>Fornecer informações verdadeiras, precisas e atualizadas durante o cadastro.</li>
                    <li>Manter a confidencialidade de sua senha de acesso, sendo o único responsável por todas as atividades que ocorram em sua conta.</li>
                    <li>Utilizar a plataforma de forma ética e legal, não tentando burlar os sistemas de segurança ou de agendamento.</li>
                    <li>Comparecer aos horários agendados ou realizar o cancelamento com a devida antecedência, conforme permitido pela Plataforma.</li>
                </ul>

                <h2>3. Política de Privacidade e Conformidade com a LGPD</h2>
                <p>A privacidade dos nossos Usuários é fundamental. Esta seção detalha como coletamos, usamos e protegemos seus dados pessoais, em total conformidade com a Lei Geral de Proteção de Dados (LGPD - Lei nº 13.709/2018).</p>

                <h3>3.1. Dados Pessoais Coletados</h3>
                <p>Para a prestação dos serviços, coletamos os seguintes dados pessoais do Usuário (Cliente):</p>
                <ul>
                    <li>**Dados de Identificação:** Nome Completo, E-mail, Número de Telefone/Celular.</li>
                    <li>**Dados de Localização:** CEP, Rua, Número, Complemento, Bairro, Cidade e UF.</li>
                    <li>**Dados de Autenticação:** Senha (armazenada de forma criptografada).</li>
                </ul>

                <h3>3.2. Finalidade do Tratamento dos Dados</h3>
                <p>Os dados coletados são utilizados estritamente para as seguintes finalidades:</p>
                <ul>
                    <li>**Execução do Serviço (Agendamento):** Identificar o cliente e associá-lo a um horário com o Barbeiro selecionado.</li>
                    <li>**Comunicação (Obrigatória):** Enviar e-mails transacionais de confirmação e cancelamento de agendamento. O E-mail é compartilhado com o provedor de e-mail (Operador) apenas para esta finalidade.</li>
                    <li>**Identificação (Login):** Permitir o acesso seguro do Usuário à sua conta.</li>
                    <li>**Facilitação (Endereço):** O endereço é coletado para fins cadastrais e pode ser utilizado para futuras funcionalidades, não sendo obrigatório para o agendamento em si (salvo quando o serviço for a domicílio).</li>
                </ul>

                <h3>3.3. Compartilhamento de Dados</h3>
                <p>Nós **não** vendemos, alugamos ou negociamos seus dados pessoais com terceiros.</p>
                <p>O compartilhamento ocorre apenas nas seguintes circunstâncias, essenciais para o funcionamento do serviço:</p>
                <ul>
                    <li>**Com o Barbeiro Selecionado:** O Barbeiro com quem o agendamento foi realizado terá acesso ao Nome do Cliente e aos serviços selecionados para poder prestar o atendimento.</li>
                    <li>**Com Provedores de Serviço (Operadores):** O E-mail do Cliente e do Barbeiro são compartilhados com nosso provedor de serviços de e-mail (ex: SendGrid) com a única finalidade de enviar as notificações de agendamento.</li>
                </ul>

                <h3>3.4. Direitos do Titular (Usuário)</h3>
                <p>Conforme a LGPD, o Usuário (titular dos dados) tem o direito de, a qualquer momento, acessar a Plataforma e:</p>
                <ul>
                    <li>**Acessar e Corrigir:** Visualizar e atualizar seus dados pessoais (através da tela "Editar Perfil").</li>
                    <li>**Excluir (Anonimização):** Solicitar a exclusão de sua conta (através da opção "Deletar Conta"). Ao fazer isso, seus dados pessoais serão anonimizados ou excluídos permanentemente do nosso banco de dados, respeitadas as obrigações legais de guarda.</li>
                </ul>

                <h2>4. Cancelamento e Encerramento de Conta</h2>
                <p>O Usuário pode, a qualquer momento, cancelar um agendamento futuro através do seu painel ("Dashboard"). O Usuário também pode excluir sua conta permanentemente através do menu de configurações.</p>
                <p>Reservamo-nos o direito de suspender ou encerrar a conta de qualquer Usuário que viole estes Termos ou utilize a plataforma de má-fé.</p>

                <h2>5. Limitação de Responsabilidade</h2>
                <p>A Plataforma é fornecida "no estado em que se encontra". Não nos responsabilizamos pela qualidade do serviço de barbearia prestado pelo profissional, que é um prestador independente. Nossa responsabilidade limita-se a garantir o funcionamento correto do sistema de agendamento e a proteção dos dados conforme descrito nesta política.</p>

                <h2>6. Alterações nos Termos</h2>
                <p>Podemos atualizar estes Termos periodicamente. Caso haja mudanças significativas, notificaremos os Usuários através de e-mail ou por um aviso visível na Plataforma.</p>

                <h2>7. Foro</h2>
                <p>Para dirimir quaisquer controvérsias oriundas destes Termos, fica eleito o foro da comarca de Mogi das Cruzes, SP, Brasil, com exclusão de qualquer outro, por mais privilegiado que seja.</p>
            
            </main>
        </div>
    );
}

export default TermosDeUso;