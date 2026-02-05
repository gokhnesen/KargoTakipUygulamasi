import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import 'zone.js';

// localhost'ta http ile açıldıysa https'e yönlendir (URL görünümü https olsun)
if (typeof window !== 'undefined' && window.location?.protocol === 'http:' && window.location?.hostname === 'localhost') {
  window.location.replace(`https://${window.location.host}${window.location.pathname}${window.location.search}${window.location.hash}`);
} else {
  bootstrapApplication(App, appConfig).catch((err) => console.error(err));
}
