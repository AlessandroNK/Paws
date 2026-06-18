import "./Calendar.css"
import {useEffect, useRef, useState} from "react";
import {Components, Day, Result} from "../types/CommonTypes.ts";
import * as LanguageService from "../services/LanguageService.ts";
import {type Appointment, TimePeriod} from "../types/SystemTypes.ts";
import * as AppointmentsService from "../services/AppointmentService.ts";
import PushMessagesUi from "../component/PushMessagesUi.tsx";
import {MessageDuration, MessageMood, MessageType, UiMessage} from "../types/MessageTypes.ts";
import TimePeriodCard from "../component/TimePeriodCard.tsx";
import MenuBar from "../component/MenuBar.tsx";

function Calendar() {
    // Variables
    // -----------------------------------------------------------------------------------------------------------------
    const isFetchingApi = useRef(false);
    const [selectedDate] = useState(new Day(2026, 6, 18));
    const [appointmentsTitle, setAppointmentsTitle] = useState("");
    const [appointmentDay, setAppointmentDay] = useState("");
    const [appointmentConnector, setAppointmentConnector] = useState("");
    const [appointmentsYear, setAppointmentsYear] = useState("");
    const [appointmentsError, setAppointmentsError] = useState("");
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
            let result = await LanguageService.getTranslationAsync(Components.CALENDAR, "APPOINTMENTS_TITLE");
            setAppointmentsTitle(result.data ?? "Available appointments for");

            result = await LanguageService.getTranslationAsync(Components.CALENDAR, "OF");
            setAppointmentDay(`${selectedDate.getDayOfWeekName()} ${selectedDate.getDayOfWeek()}`);
            setAppointmentConnector(result.data ?? "of");
            setAppointmentsYear(`${selectedDate.getMonthName()} ${result.data ?? "of"} ${selectedDate.getYear()}`);
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
            ) {
                const errorResult = await LanguageService.getTranslationAsync(Components.CALENDAR, "ERROR_GETTING_APPOINTMENTS");
                setAppointmentsError(errorResult.data ?? "Error getting appointments");
                return
            }

            if (!result.data || result.data.length <= 0) {
                const errorResult = await LanguageService.getTranslationAsync(Components.CALENDAR, "NO_AVAILABLE_APPOINTMENTS_FOUND");
                setAppointmentsError(errorResult.data ?? "No available appointments found");
                return;
            }

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
        if (appointments.length <= 0) return (
            <div className={"no-appointments"}>
                <h2>{appointmentsError}</h2>
            </div>
        );

        const periods: TimePeriod[] = []
        appointments.forEach(appointment => {
            // Find the time period
            const targetHour = appointment.startTime;
            let timePeriod = periods.find(
                period => period.startTime.getHours() === targetHour.getHours()
            );
            if (!timePeriod) {
                timePeriod = new TimePeriod(
                    `${appointment.startTime.getHours()}:00 - ${appointment.startTime.getHours() + 1}:00`,
                    appointment.startTime,
                    appointment.startTime
                )
                periods.push(timePeriod)
            }

            timePeriod.appointments.push(appointment);
        });

        // First, we need to group
        return (
            <div className={"appointments-container"}>
                {periods.map((period) => (
                    <TimePeriodCard key={period.id} timePeriod={period}/>
                ))}
            </div>
        )
    }

    // Return
    // -----------------------------------------------------------------------------------------------------------------
    return (
        <>
            <div className={"calendar-page"}>
                <section className={"calendar-header"}>
                    <MenuBar/>
                </section>
                <div className="appointments-section">
                    <div className={"appointments-section-container"}>
                        <div className={"appointments-header"}>
                            <h1>{appointmentsTitle}</h1>
                            <h1>
                                <span
                                    className={"font-bold main-gradient-text"}>{appointmentDay}</span> {appointmentConnector}
                            </h1>
                            <h1>{appointmentsYear}</h1>
                        </div>

                        <div className={"appointments-container"}>
                            {generateAppointments()}
                        </div>
                    </div>
                </div>
            </div>
            <PushMessagesUi messages={pushMessages}/>
        </>
    );
}

export default Calendar;