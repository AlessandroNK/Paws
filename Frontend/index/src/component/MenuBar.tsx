import "./MenuBar.css";
import Logo from "./Logo.tsx";
import {User} from "../types/SystemTypes.ts";
import {useState} from "react";


interface Props {
    user: User | null
}

function MenuBar(props: Props) {
    // Variables
    // -----------------------------------------------------------------------------------------------------------------
    const [menuBarProfileClass] = useState(props.user ? "menu-bar-profile logged-in" : "menu-bar-profile logged-out");

    // -----------------------------------------------------------------------------------------------------------------
    const handleProfileMouseEnter = () => {
        // Handle mouse enter event for the profile picture
        console.log("Mouse entered profile picture area");
    }

    // Return
    // -----------------------------------------------------------------------------------------------------------------
    function addProfilePicture() {
        if (props.user) {
            return (
                <div className={menuBarProfileClass}>
                    <p className={"text - 5lvl"}>{props.user.getInitials()}</p>
                </div>
            )
        } else {
            return (
                <div className={menuBarProfileClass}
                        onMouseEnter={handleProfileMouseEnter}>
                    <svg className={"icon-small icon-main-color"} data-name="Layer 1" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 583.33 583.33">
                        <path d="M291.67,583.33c-13.81,0-25-11.19-25-25s11.19-25,25-25c73.73,0,142.5-33.04,188.67-90.64,8.64-10.77,24.37-12.5,35.14-3.87,10.77,8.64,12.5,24.37,3.87,35.14-55.72,69.5-138.71,109.36-227.69,109.36ZM558.33,316.67c-13.81,0-25-11.19-25-25,0-133.26-108.41-241.67-241.67-241.67-13.81,0-25-11.19-25-25S277.86,0,291.67,0c77.91,0,151.15,30.34,206.24,85.43,55.09,55.09,85.43,128.33,85.43,206.24,0,13.81-11.19,25-25,25Z"/>
                        <path d="M258.33,416.67c-6.4,0-12.8-2.44-17.68-7.32-9.76-9.76-9.76-25.59,0-35.36l57.32-57.32H25c-13.81,0-25-11.19-25-25s11.19-25,25-25h272.98l-57.32-57.32c-9.76-9.76-9.76-25.59,0-35.36,9.76-9.76,25.59-9.76,35.36,0l100,100c2.47,2.47,4.31,5.32,5.53,8.36,0,0,0,0,0,0,0,.02.01.04.02.06,1.03,2.59,1.64,5.4,1.75,8.33,0,0,0,.02,0,.03,0,.03,0,.07,0,.1,0,.26.01.52.01.79s0,.53-.01.79c0,.03,0,.06,0,.1,0,0,0,.02,0,.03-.11,2.93-.72,5.74-1.75,8.33,0,.02-.02.04-.03.06h0c-1.22,3.05-3.06,5.9-5.53,8.37l-100,100c-4.88,4.88-11.28,7.32-17.68,7.32Z"/>
                    </svg>
                </div>
            )
        }
    }


    return (
        <div className={"menu-bar"}>
            <Logo className={"menu-bar-logo"}/>
            <div className={"menu-bar-items"}>
                <a className={"text-4lvl"} href={"/calendar"}>Citas</a>
            </div>
                {addProfilePicture()}
        </div>
    )
}

export default MenuBar;