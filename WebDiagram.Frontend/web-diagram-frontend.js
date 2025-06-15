class WebDiagramFrontend extends HTMLElement {
    connectedCallback() {
        alert("Hello from WebDiagramFrontend!");
    }
  }
  
  customElements.define('web-diagram-frontend', WebDiagramFrontend);