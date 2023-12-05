import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import Chart from 'chart.js/auto';
import {CycleService} from "../services/cycle.service";
import {CycleAnalyticsDto, PeriodCycleStatsDto} from '../interfaces/analytics.interface';


@Component({
  selector: 'app-analytics',
  templateUrl: './analytics.component.html',
  styleUrls: ['./analytics.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class AnalyticsComponent implements OnInit {
  public analytics: CycleAnalyticsDto[] = [];
  public allCycleDays: number[] = [];
  public allPeriodDays: number[] = [];
  public allNonPeriodDays: number[] = [];
  public periodCycleStats: PeriodCycleStatsDto | undefined;
  public selectedOption: number = 3
  public options: number[] = [3, 6, 9, 12];
  public chart?: Chart | null;

  constructor(private cycleService: CycleService) {

  }

  async createChart(numberOfCycles: number) {

    this.analytics = await this.cycleService.getAnalytics(numberOfCycles);
    if (this.analytics.length == 0) return;

    this.allCycleDays = this.getTotalCycleDays(this.analytics);
    this.allPeriodDays = this.getTotalPeriodDays(this.analytics);
    this.allNonPeriodDays = this.getNonPeriodDays(this.analytics);

    if (this.chart) {
      this.chart.destroy();
      this.chart = null;
    }

    const chart = new Chart('Cycle analytics', {
      type: 'bar',
      data: {
        labels: this.allCycleDays,
        datasets: [
          {
            label: "Period",
            data: this.allPeriodDays,
            backgroundColor: [
              'rgb(246, 203, 209, 0.2)'
            ],
            borderColor: [
              'rgb(246, 203, 209, 1)'
            ],
            borderWidth: 2
          },
          {
            label: "Non period",
            data: this.allNonPeriodDays,
            backgroundColor: [
              'rgb(112, 172, 199, 0.2)'
            ],
            borderColor: [
              'rgb(112, 172, 199, 1)'
            ],
            borderWidth: 2
          }
        ]
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        scales: {
          x: {
            stacked: true,
          },
          y: {
            stacked: true
          }
        }
      }
    });
  }

  async ngOnInit(): Promise<void> {
    await this.createChart(5); //TODO: make this come from user input
    this.periodCycleStats = await this.cycleService.getUserStats();
  }

  getTotalCycleDays(cycleAnalytics: CycleAnalyticsDto[]) {
    return cycleAnalytics.map(a => a.EndDate.getTime() - a.StartDate.getTime());
  }

  getTotalPeriodDays(cycleAnalytics: CycleAnalyticsDto[]) {
    return cycleAnalytics.map(a => a.PeriodDays.length);
  }

  getNonPeriodDays(cycleAnalytics: CycleAnalyticsDto[]) {
    return cycleAnalytics.map(a => a.EndDate.getTime() - (a.PeriodDays.length * 86400000) - a.StartDate.getTime());
  }

  async onOptionsSelected(item: number) {
    this.selectedOption = item;
    await this.createChart(item);
  }

  getCurrentCycleLength() : number {
    return this.periodCycleStats?.currentCycleLength ?? 0;
  }

  getAverageCycleLength() : number {
    return this.periodCycleStats?.averageCycleLength ?? 0;
  }

  getAveragePeriodLength() : number {
    return this.periodCycleStats?.averagePeriodLength ?? 0;
  }
}


