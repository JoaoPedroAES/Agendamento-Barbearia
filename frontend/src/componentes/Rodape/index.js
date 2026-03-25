import styles from './Rodape.module.css';
import iconEmail from './iconemail.png';
import iconPhone from './iconphone.png';

function Rodape() {
  return (
    <footer className={styles.rodape}>
      <h2 className={styles.titulo}>CONTATOS</h2>
      <p className={styles.descricao}>
        Tire suas dúvidas ou agende seu horário com facilidade. 
        Fale conosco pelo telefone,
        envie um e-mail ou acesse nosso site para saber mais sobre nossos serviços e promoções.
      </p>

      <div className={styles.contatos}>
        <div className={styles.card}>
          <img src={iconEmail} alt="Email" className={styles.icone} />
          <h3>E MAIL</h3>
          <a href="mailto:teste@gmail.com" className={styles.link}>
            teste@gmail.com
          </a>
          <p>
            <a href="mailto:teste2@gmail.com" className={styles.link}>
                teste2@gmail.com
            </a>
          </p>
        </div>

        <div className={styles.card}>
          <img src={iconPhone} alt="WhatsApp" className={styles.icone} />
          <h3>TELEFONE</h3>
          <a 
            href="https://wa.me/55115551212" 
            target="_blank" 
            rel="noopener noreferrer"
            className={styles.link}
          >
            (+11) 662 3535
          </a>
          <p>
            <a 
                href="https://wa.me/55116623535" 
                target="_blank" 
                rel="noopener noreferrer"
                className={styles.link}
            >
                (+11) 555 1212
            </a>
          </p>
        </div>
      </div>

      <div className={styles.copy}>
        © Copyright Barbershop 2025
      </div>
    </footer>
  );
}

export default Rodape;
