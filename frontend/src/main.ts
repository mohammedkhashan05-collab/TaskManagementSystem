import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { AppModule } from './app/app.module';

platformBrowserDynamic().bootstrapModule(AppModule)
  .catch(err => {
    console.error('Error bootstrapping Angular application:', err);
    console.error('Error details:', JSON.stringify(err, null, 2));
  });

