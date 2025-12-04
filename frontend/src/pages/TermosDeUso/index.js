import React from 'react';
import styles from './TermosDeUso.module.css';

function TermosDeUso() {

    return (
        <div className={styles.page}>
            <header className={styles.header}>
                <h1>Termos de Uso e Política de Privacidade</h1>
            </header>

            <main className={styles.content}>
                
                <p className={styles.lastUpdate}>Última atualização: 09 de Novembro de 2025</p>

                <p>Bem-vindo! Estes termos explicam de forma simples como funciona a nossa plataforma de agendamento e como cuidamos dos seus dados. Ao criar sua conta, você concorda com as regras abaixo.</p>

                <h2>1. O que é a Plataforma?</h2>
                <p>Nós fornecemos uma ferramenta digital para facilitar sua vida. Aqui, você (`Cliente`) pode ver a agenda dos profissionais (`Barbeiros`), escolher o serviço que deseja e marcar seu horário de forma rápida e segura.</p>

                <h2>2. Suas Responsabilidades</h2>
                <p>Para usar o sistema, você precisa:</p>
                <ul>
                    <li>Fornecer dados reais no cadastro (seu nome e contato verdadeiros).</li>
                    <li>Cuidar da sua senha (não a compartilhe com ninguém).</li>
                    <li>Usar o sistema de forma honesta, sem tentar enganar o agendamento.</li>
                    <li><strong>Compromisso:</strong> Comparecer no horário marcado ou cancelar com antecedência se não puder ir.</li>
                </ul>

                <h2>3. Seus Dados e a LGPD (Privacidade)</h2>
                <p>Levamos sua privacidade a sério. Respeitamos a <strong>Lei Geral de Proteção de Dados (LGPD)</strong> e usamos seus dados apenas para o funcionamento do agendamento.</p>

                <h3>3.1. Que dados pedimos?</h3>
                <ul>
                    <li><strong>Identificação:</strong> Nome, E-mail e Celular (para saber quem você é).</li>
                    <li><strong>Localização:</strong> Seu endereço (para completar o cadastro).</li>
                    <li><strong>Segurança:</strong> Sua senha (que é salva com <strong>Criptografia</strong>, ou seja, ninguém consegue ler, nem nós).</li>
                </ul>

                <h3>3.2. Para que usamos esses dados?</h3>
                <ul>
                    <li><strong>Agendar:</strong> Para dizer ao barbeiro quem marcou o horário.</li>
                    <li><strong>Avisar:</strong> Para enviar e-mails de confirmação ou cancelamento.</li>
                    <li><strong>Entrar:</strong> Para você fazer login na sua conta.</li>
                </ul>

                <h3>3.3. Compartilhamos seus dados?</h3>
                <p>Nós <strong>nunca</strong> vendemos seus dados.</p>
                <p>Seus dados só aparecem para:</p>
                <ul>
                    <li><strong>O Barbeiro:</strong> Ele precisa saber seu nome para lhe atender.</li>
                    <li><strong>Sistema de E-mail:</strong> Nosso provedor de envio de e-mails recebe seu endereço apenas para entregar as notificações de agendamento.</li>
                </ul>

                <h3>3.4. Seus Direitos</h3>
                <p>Você é o dono dos seus dados. A qualquer momento, você pode:</p>
                <ul>
                    <li><strong>Corrigir:</strong> Mudar suas informações no menu "Editar Perfil".</li>
                    <li><strong>Excluir:</strong> Usar a opção "Deletar Conta". Se fizer isso, realizamos a <strong>Anonimização</strong> ou exclusão dos seus dados do nosso sistema, mantendo apenas o que for exigido por lei.</li>
                </ul>

                <h2>4. Cancelamentos</h2>
                <p>Imprevistos acontecem. Você pode cancelar um agendamento pelo seu painel ("Dashboard"). Também pode excluir sua conta quando quiser. Se notarmos uso de má-fé (como agendar e faltar repetidamente sem avisar), podemos suspender a conta.</p>

                <h2>5. Nossa Responsabilidade</h2>
                <p>Nós garantimos que o <strong>sistema de agendamento</strong> funcione corretamente e proteja seus dados. A qualidade do corte e o atendimento presencial são de total responsabilidade do Barbeiro, que é um profissional independente.</p>

                <h2>6. Mudanças nos Termos</h2>
                <p>Se precisarmos mudar algo importante nestas regras, avisaremos você por e-mail ou por um aviso claro aqui na plataforma.</p>

                <h2>7. Foro (Onde resolvemos problemas)</h2>
                <p>Fica eleito o foro da comarca de <strong>Mogi das Cruzes, SP</strong>, para resolver quaisquer dúvidas legais sobre estes termos.</p>
            
            </main>
        </div>
    );
}

export default TermosDeUso;