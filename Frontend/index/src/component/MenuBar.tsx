import "./MenuBar.css";
import Logo from "./Logo.tsx";

interface Props {
    selected: string
}

function MenuBar() {

    return (
        <div className={"menu-bar"}>
            <Logo className={"menu-bar-logo"}/>
            <div className={"menu-bar-items"}>
                <a className={"text-4lvl"} href={"/calendar"}>Citas</a>
            </div>
            <div className={"menu-bar-profile"}>
                <p className={"text-5lvl"}>JD</p>
            </div>
        </div>
    )
}

export default MenuBar;