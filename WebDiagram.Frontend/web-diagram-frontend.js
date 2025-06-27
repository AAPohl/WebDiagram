import { createSignalRConnection } from './signalr-client.js';
import { ViewportController } from './viewport-controller.js';
import { HoverController } from './hover-controller.js';

class WebDiagramFrontend extends HTMLElement {
    async connectedCallback() {
        if (typeof signalR === 'undefined') {
            await this.loadScript("https://cdn.jsdelivr.net/npm/@microsoft/signalr@7.0.5/dist/browser/signalr.min.js");
        }

        this.baseUrl = this.getAttribute("source").replace(/\/$/, '');
        this.instancePath = `/${this.getAttribute("id")}`;

        fetch(`${this.baseUrl}${this.instancePath}/config`)
            .then(res => res.json())
            .then(cfg => {
                this.margin = cfg.margin;
            });

        this.img = document.createElement('img');
        this.img.id = "view";
        this.img.width = this.getAttribute("width") || 100;
        this.img.height = this.getAttribute("height") || 100;
        this.img.style.border = "2px solid #555";
        this.img.style.background = "black";
        this.appendChild(this.img);

        // HoverController initialisieren
        this.hoverController = new HoverController({
            imgElement: this.img,
            getMargin: () => this.margin,
            getViewPort: () => this.viewportController.viewPort,
            updateHover: (hoverPos) => this.updateHover(hoverPos)
        });

        // ViewportController initialisieren (inkl. Event-Registrierung intern)
        this.viewportController = new ViewportController({
            imgElement: this.img,
            getMargin: () => this.margin,
            updateViewPort: (vp) => this.updateViewPort(vp),
            updateSize: (w, h) => this.updateSize(w, h)
        });

        this.viewportController.setInitialViewPort({ xMin: 0, yMin: 0, xMax: 1, yMax: 1 });
        this.updateSize(this.img.width, this.img.height);

        // SignalR-Verbindung aufbauen
        this.connection = await createSignalRConnection(
            `${this.baseUrl}/diagramhub`,
            image => this.img.src = image
        );
        await this.connection.invoke("JoinInstanceGroup", this.getAttribute("id"));
    }

    disconnectedCallback() {
        if (this.viewportController) {
            this.viewportController.destroy(); // Event-Listener sauber entfernen
        }

        if (this.hoverController) {
            this.hoverController.destroy(); // ebenfalls aufräumen
        }

        if (this.connection) {
            this.connection.stop(); // SignalR-Verbindung trennen
        }
    }

    loadScript(src) {
        return new Promise((resolve, reject) => {
            const script = document.createElement('script');
            script.src = src;
            script.onload = resolve;
            script.onerror = reject;
            document.head.appendChild(script);
        });
    }

    setInitialViewPort(viewPort) {
        this.viewportController.setInitialViewPort(viewPort);
    }

    updateViewPort(viewPort) {
        const url = `${this.baseUrl}${this.instancePath}/updateViewPort?xMin=${viewPort.xMin}&xMax=${viewPort.xMax}&yMin=${viewPort.yMin}&yMax=${viewPort.yMax}`;
        fetch(url);
    }

    updateSize(width, height) {
        const url = `${this.baseUrl}${this.instancePath}/updateSize?width=${width}&height=${height}`;
        fetch(url);
    }

    updateHover(hoverPos) {
        const url = `${this.baseUrl}${this.instancePath}/updateHover?x=${hoverPos.x}&y=${hoverPos.y}`;
        fetch(url);
    }
}

customElements.define('web-diagram-frontend', WebDiagramFrontend);
