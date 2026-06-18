import Logo from "./Logo.tsx";
import "./SideBar.css";
import { useEffect, useState } from "react";
import * as LanguageService from "../services/LanguageService.ts";
import {Components} from "../types/CommonTypes.ts";

function SideBar() {
    // Variables
    // -----------------------------------------------------------------------------------------------------------------
    const [dayOfWeek] = useState(LanguageService.getDayOfWeek(new Date().getDay()));
    const [date] = useState(new Date().toLocaleDateString());
    const [appointmentsText, setAppointmentsText] = useState("");


    // -----------------------------------------------------------------------------------------------------------------
    useEffect(() => {
        async function loadTranslation() {
            const result = await LanguageService.getTranslationAsync(Components.SIDEBAR, "APPOINTMENTS");
            setAppointmentsText(result.data ?? "Appointments");
        }

        loadTranslation();
    }, []);

    // Return
    // -----------------------------------------------------------------------------------------------------------------
    return (
        <section className="sidebar">
            <div className={"sidebar-content"}>
                <Logo className={"main-logo"}/>

                <div className={"date-info mb-4"}>
                    <p className="text-2lvl font-semi-bold">{dayOfWeek}</p>
                    <p className="text-5lvl">{date}</p>
                </div>

                <ul className={"erase-list-style"}>
                    <li>
                        <a href="#" className="text-blue-500 hover:underline">
                            <div className={"flex text-3lvl items-center gap-3 selected"}>
                            <span>
                                <svg className={"icon-small"} data-name="Layer 1" xmlns="http://www.w3.org/2000/svg"
                                     viewBox="0 0 11.94 14.03">
                                  <path
                                      d="M9.83,2.09h-.35V.6c0-.33-.27-.6-.6-.6s-.6.27-.6.6v1.49h-1.72V.6c0-.33-.27-.6-.6-.6s-.6.27-.6.6v1.49h-1.72V.6c0-.33-.27-.6-.6-.6s-.6.27-.6.6v1.49h-.35c-1.16,0-2.11.95-2.11,2.11v7.72c0,1.16.95,2.11,2.11,2.11h7.72c1.16,0,2.11-.95,2.11-2.11v-7.72c0-1.16-.95-2.11-2.11-2.11ZM10.75,11.92c0,.5-.41.92-.92.92H2.11c-.5,0-.92-.41-.92-.92v-7.72c0-.5.41-.92.92-.92h.35v1c0,.33.27.6.6.6s.6-.27.6-.6v-1h1.72v1c0,.33.27.6.6.6s.6-.27.6-.6v-1h1.72v1c0,.33.27.6.6.6s.6-.27.6-.6v-1h.35c.5,0,.92.41.92.92v7.72Z"/>
                                </svg>
                            </span>
                                {appointmentsText}
                            </div>
                        </a>
                    </li>
                </ul>
            </div>
        </section>
    );
}

export default SideBar;