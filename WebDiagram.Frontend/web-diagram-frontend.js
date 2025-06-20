class WebDiagramFrontend extends HTMLElement {
    connectedCallback() {
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

        this.throttledUpdateImage = this.throttle(this.updateImage.bind(this), 30);

        this.img.addEventListener("mousedown", this.startDrag.bind(this));
        window.addEventListener("mouseup", this.stopDrag.bind(this));
        window.addEventListener("mousemove", this.dragMove.bind(this));
        window.addEventListener("keydown", this.handleKeyDown.bind(this));
        this.img.addEventListener("wheel", this.handleWheel.bind(this));
    }

    setInitialViewPort(viewPort) {
        this.viewPort = viewPort;
        this.updateImage(viewPort);  // Initial synchron
    }

    updateImage(viewPort) {
        const url = `${this.source}/render?xMin=${viewPort.xMin}&xMax=${viewPort.xMax}&yMin=${viewPort.yMin}&yMax=${viewPort.yMax}&width=${this.img.width}&height=${this.img.height}`;
    
        fetch(url)
          .then(res => {
            if (!res.ok) throw new Error(); // Fehlerhafte Antworten (z.B. 500) still ignorieren
            return res.blob();
          })
          .then(blob => {
            if (blob && blob.size > 0) {
              const objectURL = URL.createObjectURL(blob);
              this.displayImage(objectURL);
            }
          })
          .catch(() => {
            // Fehler (z. B. 500) still ignorieren – kein Logging
          });
    }
    

    displayImage(objectURL) {
        const img = this.img;
        img.onload = () => {
            URL.revokeObjectURL(img.src);  // Speicher freigeben
        };
        img.src = objectURL;
    }

    throttle(func, delay) {
        let lastCall = 0;
        return (...args) => {
            const now = Date.now();
            if (now - lastCall >= delay) {
                lastCall = now;
                func.apply(this, args);
            }
        };
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
    
        this.throttledUpdateImage(this.viewPort);
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
    
        this.updateImage(this.viewPort);  // synchron, da seltener
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
            this.throttledUpdateImage(this.viewPort);
            e.preventDefault();
            return;
        }
    
        if (mouseX <= this.margin.left) {
            const worldY = this.viewPort.yMax - ((mouseY - this.margin.top) / (rect.height - this.margin.top - this.margin.bottom)) * height;
            const newHeight = height * scale;
            const newYMax = worldY + ((mouseY - this.margin.top) / (rect.height - this.margin.top - this.margin.bottom)) * newHeight;
            this.viewPort.yMin = newYMax - newHeight;
            this.viewPort.yMax = newYMax;
            this.throttledUpdateImage(this.viewPort);
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
            this.throttledUpdateImage(this.viewPort);
            e.preventDefault();
        }
    }    
}

customElements.define('web-diagram-frontend', WebDiagramFrontend);
