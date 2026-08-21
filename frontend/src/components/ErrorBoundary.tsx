import { Component, type ReactNode } from "react";

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
}

export class ErrorBoundary extends Component<Props, State> {
  state: State = { hasError: false };

  static getDerivedStateFromError(): State {
    return { hasError: true };
  }

  componentDidCatch(error: unknown) {
    console.error("Uygulama hatası:", error);
  }

  render() {
    if (this.state.hasError) {
      return (
        <div style={{ display: "flex", minHeight: "100vh", alignItems: "center", justifyContent: "center", padding: 24, textAlign: "center", fontFamily: "sans-serif" }}>
          <div>
            <p style={{ fontWeight: 600, marginBottom: 8 }}>Bir şeyler ters gitti.</p>
            <p style={{ color: "#6b8299", fontSize: 14 }}>Sayfayı yenilemeyi deneyin.</p>
          </div>
        </div>
      );
    }
    return this.props.children;
  }
}
