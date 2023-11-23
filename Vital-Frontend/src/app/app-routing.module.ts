import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {DashboardComponent} from "./dashboard/dashboard.component";
import {AnalyticsComponent} from "./analytics/analytics.component";
import {LoginComponent} from "./login/login.component";
import {authGuard} from "./gurads/auth.guard";
import {RegisterComponent} from "./register/register.component";
import {AddMetricPageComponent} from "./add-metric-page/add-metric-page.component";

const routes: Routes = [
    {path: '', redirectTo: 'dashboard', pathMatch: 'full'},
    {
        path: 'dashboard',
        component: DashboardComponent,
        canActivate: [authGuard]
    },
    {
        path: 'analytics',
        component: AnalyticsComponent,
        canActivate: [authGuard]
    },
    {
      path: 'add-metric',
      component: AddMetricPageComponent,
      pathMatch: 'full'
    },
    {path: 'login', component: LoginComponent},
    {path: 'register', component: RegisterComponent},
    {path: '**', redirectTo: 'dashboard'},
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})
export class AppRoutingModule {
}
