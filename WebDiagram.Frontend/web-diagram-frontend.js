import { createSignalRConnection } from './signalr-client.js';
import { ViewportController } from './viewport-controller.js';

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

        
        // ViewportController initialisieren
        this.viewportController = new ViewportController({
            imgElement: this.img,
            getMargin: () => this.margin,
            updateViewPort: (vp) => this.updateViewPort(vp)
        });

        // Event Delegation an ViewportController
        this.img.addEventListener("mousedown", e => this.viewportController.startDrag(e));
        window.addEventListener("mouseup", () => this.viewportController.stopDrag());
        window.addEventListener("mousemove", e => this.viewportController.dragMove(e));
        window.addEventListener("keydown", e => this.viewportController.handleKeyDown(e));
        this.img.addEventListener("wheel", e => this.viewportController.handleWheel(e));
        
        this.viewportController.setInitialViewPort({ xMin: 0, yMin: 0, xMax: 1, yMax: 1 });
        this.updateSize(this.img.width, this.img.height);
             
        this.connection = await createSignalRConnection(
            `${this.baseUrl}/diagramhub`,
            image => this.img.src = image
        );
        await this.connection.invoke("JoinInstanceGroup", this.getAttribute("id"));

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
}

customElements.define('web-diagram-frontend', WebDiagramFrontend);
