import "./Calendar.css"
import {useEffect, useRef, useState} from "react";
import {Components, Day, Result} from "../types/CommonTypes.ts";
import * as LanguageService from "../services/LanguageService.ts";
import {type Appointment} from "../types/SystemTypes.ts";
import * as AppointmentsService from "../services/AppointmentService.ts";
import SideBar from "../component/SideBar.tsx";
import PushMessagesUi from "../component/PushMessagesUi.tsx";
import {MessageDuration, MessageMood, MessageType, UiMessage} from "../types/MessageTypes.ts";
import AppointmentCard from "../component/AppointmentCard.tsx";

function Calendar() {
    // Variables
    // -----------------------------------------------------------------------------------------------------------------
    const isFetchingApi = useRef(false);
    const [selectedDate] = useState(new Day(2026, 6, 18));
    const [selectedDateString, setSelectedDateString] = useState("");
    const [appointmentsTitle, setAppointmentsTitle] = useState("");
    const [appointmentDatePhar, setAppointmentDatePhar] = useState("");
    const [appointments, setAppointments] = useState([] as Appointment[]);
    const [pushMessages, setPushMessages] = useState([] as UiMessage[])


    // -----------------------------------------------------------------------------------------------------------------
    async function showErrorsAsPushMessages(result: Result<unknown>) {
        const pushMessages: UiMessage[] = [];
        if (result.code) {
            const uiMessage = new UiMessage();
            uiMessage.type = result.success ? MessageType.SUCCESS : MessageType.ERROR;
            uiMessage.mood = result.success ? MessageMood.HAPPY : MessageMood.SAD;
            uiMessage.duration = MessageDuration.MEDIUM;
            uiMessage.component = result.component;
            uiMessage.code = result.code;
            pushMessages.push(uiMessage);
        }

        if (result.errors.length > 0) {
            result.errors.forEach(error => {
                for (const message of error.messages) {
                    pushMessages.push(message);
                }
            });
        }
        setPushMessages(pushMessages);
    }

    // -----------------------------------------------------------------------------------------------------------------
    useEffect(() => {
        async function loadTranslation() {
            let result = await LanguageService.getTranslation(Components.CALENDAR, "APPOINTMENTS_TITLE");
            setAppointmentsTitle(result.data ?? "Appointments' calendar");

            result = await LanguageService.getTranslation(Components.CALENDAR, "APPOINTMENT_DATE_PHAR");
            setAppointmentDatePhar(result.data ?? "You are selecting an appointment for the");

            setSelectedDateString(await selectedDate.printFormated());
        }

        async function getAppointmentsApi() {
            if (isFetchingApi.current) return;

            // Get appointments from API
            isFetchingApi.current = true;
            const result = await AppointmentsService.getAvailableAppointmentsApi(selectedDate);
            isFetchingApi.current = false;

            // Show errors up
            showErrorsAsPushMessages(result);

            // Validations
            if (
                result.code === "NO_AVAILABLE_APPOINTMENTS_FOUND" ||
                result.code === "INVALID_DATE" ||
                result.code === "APPOINTMENTS_FETCH_ERROR" ||
                result.code === "CANNOT_GET_AVAILABLE_APPOINTMENTS_FOR_PAST_DAYS"
            ) return
            if (!result.data) return;

            // Push amount message
            const uiMessage = new UiMessage();
            uiMessage.message = `Encontramos ${result.data?.length} citas disponibles para que escojas la que quieras!`;
            uiMessage.type = MessageType.SUCCESS;
            uiMessage.duration = MessageDuration.LONG;

            // Push message
            const pushMessages: UiMessage[] = [];
            pushMessages.push(uiMessage);
            setPushMessages(pushMessages);

            // Set appointments
            setAppointments(result.data);
        }

        loadTranslation();
        getAppointmentsApi();
    }, []);


    // -----------------------------------------------------------------------------------------------------------------
    function generateAppointments() {
        if (appointments.length <= 0) return null;

        // First, we need to group
        return (
            <div className="appointment-time-period">
            {appointments.map((appointment) => (
                    <AppointmentCard key={appointment.id} appointment={appointment}/>
                ))}
            </div>
        )
    }

    // Return
    // -----------------------------------------------------------------------------------------------------------------
    return (
        <>
            <div className={"calendar-page"}>
                <SideBar/>
                <div className="calendar-content">
                    <section className="calendar-section">

                    </section>

                    <section className="hours-section">

                    </section>

                    <section className="appointments-section">
                        <h1>{appointmentsTitle}</h1>
                        <h2>
                            {appointmentDatePhar} <span
                            className={"font-bold font-main-color"}>{selectedDateString}</span>
                        </h2>
                        {generateAppointments()}
                    </section>
                </div>
            </div>
            <PushMessagesUi messages={pushMessages}/>
        </>
    );
}

export default Calendar;