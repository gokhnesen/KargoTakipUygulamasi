import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import 'zone.js';

if (typeof window !== 'undefined' && window.location?.protocol === 'http:' && window.location?.hostname === 'localhost') {
  window.location.replace(`https://${window.location.host}${window.location.pathname}${window.location.search}${window.location.hash}`);
} else {
  bootstrapApplication(App, appConfig).catch(() => {});
}
