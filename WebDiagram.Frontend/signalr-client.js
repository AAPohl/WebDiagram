export function createSignalRConnection(url, onImageReceived) {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl(url)
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveImage", base64Image => {
        onImageReceived("data:image/png;base64," + base64Image);
    });

    return connection.start().then(() => connection);
}
