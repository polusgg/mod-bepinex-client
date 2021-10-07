namespace Polus.Enums
{
    public enum HudItem : byte {
        // Disables access to normal map
        MapButton,
        // Disables all "sabotage" buttons in MapBehavior
        MapSabotageButtons,
        // Disables all "close door" buttons in MapBehavior
        MapDoorButtons,
        // Disables Sabotage button (which is included into Use Button)
        SabotageButton,
        // Disables Vent button (which is included into Use Button)
        VentButton,
        // Disables whole Use button (includes Vent, Sabotage, Admin etc.)
        UseButton,
        // Disables task progress bar
        TaskProgressBar,
        // Disables popup (or rather slide-out) with tasks list
        TaskListPopup,
        // Disables report button
        ReportButton,
        // Disables Meeting button (which is included into Use button)
        CallMeetingButton,
        // Disables use of the table table
        AdminTable,
        // Hides the game code when in the lobby
        GameCode
    }
}