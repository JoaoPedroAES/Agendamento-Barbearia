import { Link } from "react-router-dom";
import styles from './Cabecalho.module.css';
import CabecalhoLink from "componentes/CabecalhoLink";

function Cabecalho() {
  return (
    <header className={styles.cabecalho}>

      <nav className={styles.nav}>
        <div className={styles.esquerdaLinks}>
          <CabecalhoLink url="./Login">Login</CabecalhoLink>
        </div>
          <div className={styles.menuLinks}>
            <a href="./">Home</a>
            <a href="./servicos">Serviços</a>
            <a href="./contatos">Contatos</a>
        </div>
        <div className={styles.direitaLinks}>
          <CabecalhoLink url="./Agendamento">Agendamento</CabecalhoLink>
        </div>
      </nav>
    </header>
  );
}

export default Cabecalho;
