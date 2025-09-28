import Cabecalho from "componentes/cabecalho";
import Logo from "./Logo.png"
import styles from './Inicio.module.css';
import Estilos from "componentes/EstilosCorte";
import Rodape from "componentes/Rodape";

function Inicio(){
    return(
<div
      style={{
        backgroundImage: "url('/img/background.png')",
        backgroundSize: "cover",
        backgroundRepeat: "no-repeat",
        backgroundPosition: "center",
        width: "100%",
        minHeight: "100vh"
      }}
    >
      <Cabecalho/>
    <div className={styles.logobarber}>
     <img id="logo" src={Logo} alt="Logo"/>
    </div>
      <Estilos/>
      <Rodape/>
</div>
    )
    
}

export default Inicio;