import styles from './Estilos.module.css';
import imgCortes from "./cortes.png";

function Estilos() {
  return (
      <section className={styles.container}>
        <div className={styles.linha}></div>
        <h2 className={styles.titulo}>ESTILOS DE CORTES</h2>
        <div className={styles.imagemContainer}>
          <img
            src={imgCortes}
            alt="Serviços da barbearia"
            className={styles.imagem}
          />
        </div>
      </section>
  );
}

export default Estilos;
