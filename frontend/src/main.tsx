import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { ErrorBoundary } from './components/ErrorBoundary.tsx'

const root = createRoot(document.getElementById('root')!)

// navigator.webdriver is set by Selenium/Playwright/Puppeteer and most headless browsers.
// A determined bot can patch this away, but it stops naive automation from loading the app.
if (navigator.webdriver) {
  root.render(
    <div style={{ display: 'flex', minHeight: '100vh', alignItems: 'center', justifyContent: 'center', padding: 24, textAlign: 'center', fontFamily: 'sans-serif', color: '#6b8299' }}>
      Otomatik tarayıcı erişimi desteklenmiyor.
    </div>,
  )
} else {
  root.render(
    <StrictMode>
      <ErrorBoundary>
        <App />
      </ErrorBoundary>
    </StrictMode>,
  )
}
