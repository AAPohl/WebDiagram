import { createSignalRConnection } from './signalr-client.js';

class WebDiagramFrontend extends HTMLElement {
    async connectedCallback() {
        if (typeof signalR === 'undefined') {
            await this.loadScript("https://cdn.jsdelivr.net/npm/@microsoft/signalr@7.0.5/dist/browser/signalr.min.js");
        }

        this.img = document.createElement('img');
        this.img.id = "view";
        this.img.width = this.getAttribute("width") || 100;
        this.img.height = this.getAttribute("height") || 100;
        this.img.style.border = "2px solid #555";
        this.img.style.background = "black";

        this.appendChild(this.img);
        this.source = this.getAttribute("source")

        fetch(`${this.source}/config`)
            .then(res => res.json())
            .then(cfg => {
                this.margin = cfg.margin;
                console.log('[Config] Margin geladen:', this.margin);
            });

        this.viewPort = { xMin: 0, yMin: 0, xMax: 1, yMax: 1 };
        this.isDragging = false;
        this.lastMousePos = { x: 0, y: 0 };

        this.stepX = 0.02;
        this.stepY = 0.02;
        this.zoomFactor = 1.1;

        this.img.addEventListener("mousedown", this.startDrag.bind(this));
        window.addEventListener("mouseup", this.stopDrag.bind(this));
        window.addEventListener("mousemove", this.dragMove.bind(this));
        window.addEventListener("keydown", this.handleKeyDown.bind(this));
        this.img.addEventListener("wheel", this.handleWheel.bind(this));
        this.updateViewPort(this.viewPort);
        this.updateSize(this.img.width, this.img.height);

        this.connection = await createSignalRConnection(
            `${this.source.replace(/\/$/, '')}/diagramhub`,
            image => this.img.src = image
        );
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
        this.viewPort = viewPort;
        this.updateViewPort(viewPort);
    }

    updateViewPort(viewPort) {
        const url = `${this.source}/updateViewPort?xMin=${viewPort.xMin}&xMax=${viewPort.xMax}&yMin=${viewPort.yMin}&yMax=${viewPort.yMax}`;
        fetch(url);
    }

    updateSize(width, height) {
        const url = `${this.source}/updateSize?width=${width}&height=${height}`;
        fetch(url);
    }

    startDrag(e) {
        this.isDragging = true;
        this.lastMousePos = { x: e.clientX, y: e.clientY };
        e.preventDefault();
    }
    
    stopDrag() {
        this.isDragging = false;
    }
    
    dragMove(e) {
        if (!this.isDragging) return;
    
        const dx = e.clientX - this.lastMousePos.x;
        const dy = e.clientY - this.lastMousePos.y;
        this.lastMousePos = { x: e.clientX, y: e.clientY };
    
        const widthPx = this.img.naturalWidth - this.margin.left - this.margin.right;
        const heightPx = this.img.naturalHeight - this.margin.top - this.margin.bottom;
    
        const xRange = this.viewPort.xMax - this.viewPort.xMin;
        const yRange = this.viewPort.yMax - this.viewPort.yMin;
    
        const scaleX = xRange / widthPx;
        const scaleY = yRange / heightPx;
    
        this.viewPort.xMin -= dx * scaleX;
        this.viewPort.xMax -= dx * scaleX;
    
        this.viewPort.yMin += dy * scaleY;
        this.viewPort.yMax += dy * scaleY;

        this.updateViewPort(this.viewPort);
        this.updateSize(this.img.width, this.img.height);
    }

    handleKeyDown(e) {
        if (!this.viewPort) return;
    
        switch (e.key) {
            case 'ArrowLeft':
                this.viewPort.xMin += this.stepX;
                this.viewPort.xMax += this.stepX;
                break;
            case 'ArrowRight':
                this.viewPort.xMin -= this.stepX;
                this.viewPort.xMax -= this.stepX;
                break;
            case 'ArrowUp':
                this.viewPort.yMin -= this.stepY;
                this.viewPort.yMax -= this.stepY;
                break;
            case 'ArrowDown':
                this.viewPort.yMin += this.stepY;
                this.viewPort.yMax += this.stepY;
                break;
            default:
                return;
        }

        this.updateViewPort(this.viewPort);
        this.updateSize(this.img.width, this.img.height);
    }

    handleWheel(e) {
        const rect = this.img.getBoundingClientRect();
        const mouseX = e.clientX - rect.left;
        const mouseY = e.clientY - rect.top;
        const width = this.viewPort.xMax - this.viewPort.xMin;
        const height = this.viewPort.yMax - this.viewPort.yMin;
    
        const scale = (e.deltaY < 0) ? (1 / this.zoomFactor) : this.zoomFactor;
    
        if (mouseY >= rect.height - this.margin.bottom) {
            const worldX = this.viewPort.xMin + ((mouseX - this.margin.left) / (rect.width - this.margin.left - this.margin.right)) * width;
            const newWidth = width * scale;
            const newXMin = worldX - ((mouseX - this.margin.left) / (rect.width - this.margin.left - this.margin.right)) * newWidth;
            this.viewPort.xMin = newXMin;
            this.viewPort.xMax = newXMin + newWidth;
            this.updateViewPort(this.viewPort);
            this.updateSize(this.img.width, this.img.height);
            e.preventDefault();
            return;
        }
    
        if (mouseX <= this.margin.left) {
            const worldY = this.viewPort.yMax - ((mouseY - this.margin.top) / (rect.height - this.margin.top - this.margin.bottom)) * height;
            const newHeight = height * scale;
            const newYMax = worldY + ((mouseY - this.margin.top) / (rect.height - this.margin.top - this.margin.bottom)) * newHeight;
            this.viewPort.yMin = newYMax - newHeight;
            this.viewPort.yMax = newYMax;
            this.updateViewPort(this.viewPort);
            this.updateSize(this.img.width, this.img.height);
            e.preventDefault();
            return;
        }
    
        const insideHorizontal = mouseX > this.margin.left && mouseX < (rect.width - this.margin.right);
        const insideVertical = mouseY > this.margin.top && mouseY < (rect.height - this.margin.bottom);
    
        if (insideHorizontal && insideVertical) {
            const worldX = this.viewPort.xMin + ((mouseX - this.margin.left) / (rect.width - this.margin.left - this.margin.right)) * width;
            const worldY = this.viewPort.yMax - ((mouseY - this.margin.top) / (rect.height - this.margin.top - this.margin.bottom)) * height;
            const newWidth = width * scale;
            const newHeight = height * scale;
            const newXMin = worldX - ((mouseX - this.margin.left) / (rect.width - this.margin.left - this.margin.right)) * newWidth;
            const newYMax = worldY + ((mouseY - this.margin.top) / (rect.height - this.margin.top - this.margin.bottom)) * newHeight;
            this.viewPort.xMin = newXMin;
            this.viewPort.xMax = newXMin + newWidth;
            this.viewPort.yMin = newYMax - newHeight;
            this.viewPort.yMax = newYMax;
            this.updateViewPort(this.viewPort);
            this.updateSize(this.img.width, this.img.height);
            e.preventDefault();
        }
    }    
}

customElements.define('web-diagram-frontend', WebDiagramFrontend);
