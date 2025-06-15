class WebDiagramFrontend extends HTMLElement {
    connectedCallback() {
        // Template erzeugen
        const img = document.createElement('img');
        img.id = "view";
        img.width = this.getAttribute("width") || 100;
        img.height = this.getAttribute("height") || 100;
        img.style.border = "2px solid #555";
        img.style.background = "black";

        this.appendChild(img);
        alert("Hello from WebDiagramFrontend!");
    }
  }
  
  customElements.define('web-diagram-frontend', WebDiagramFrontend);