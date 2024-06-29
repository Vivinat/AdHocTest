import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { ApiConsumerService } from '../api-consumer.service';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent implements OnInit, AfterViewInit {
  edible_fruit: boolean = false;
  indoor: boolean = false;
  invasive: boolean = false;
  medicinal: boolean = false;
  poisonous_to_pets: boolean = false;
  growth_rate: string = '';
  care_level: string = '';
  watering: string = '';
  sunlight: string = '';
  primaryTable: string = 'plant';
  secondaryTable: string = 'plant_details';
  displayedColumns: string[] = ['scientific_name', 'common_name'];
  dataSource = new MatTableDataSource<any>([]);
  @ViewChild(MatPaginator) paginator: MatPaginator | undefined;

  constructor(private apiConsumer: ApiConsumerService) {}

  ngOnInit(): void {
    // Nada a ser feito aqui por enquanto
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator!;
  }

  switchTrueFalseEdible() {
    this.edible_fruit = !this.edible_fruit;
  }

  switchTrueFalseIndoor() {
    this.indoor = !this.indoor;
  }

  switchTrueFalseInvasive() {
    this.invasive = !this.invasive;
  }

  switchTrueFalseMedicinal() {
    this.medicinal = !this.medicinal;
  }

  switchTrueFalsePoisonous() {
    this.poisonous_to_pets = !this.poisonous_to_pets;
  }

  changeTable(event: Event) {
    const target = event.target as HTMLSelectElement;
    this.primaryTable = target.value;
  }

  changeTableSecondary(event: Event) {
    const target = event.target as HTMLSelectElement;
    this.secondaryTable = target.value;
    this.clearFilters();
  }

  clearFilters() {
    this.edible_fruit = false;
    this.indoor = false;
    this.invasive = false;
    this.medicinal = false;
    this.poisonous_to_pets = false;
    this.growth_rate = '';
    this.care_level = '';
    this.watering = '';
    this.sunlight = '';
  }

  changeGrowthRate(event: Event) {
    const target = event.target as HTMLSelectElement;
    this.growth_rate = target.value;
  }

  changeCareLevel(event: Event) {
    const target = event.target as HTMLSelectElement;
    this.care_level = target.value;
  }

  changeWatering(event: Event) {
    const target = event.target as HTMLSelectElement;
    this.watering = target.value;
  }

  changeSunlight(event: Event) {
    const target = event.target as HTMLSelectElement;
    this.sunlight = target.value;
  }

  search() {
    let query = `${this.primaryTable}:${this.secondaryTable}:`;
    if (this.edible_fruit) {
      query += 'edible_fruit=true;';
    }
    if (this.indoor) {
      query += 'indoor=true;';
    }
    if (this.invasive) {
      query += 'invasive=true;';
    }
    if (this.medicinal) {
      query += 'medicinal=true;';
    }
    if (this.poisonous_to_pets) {
      query += 'poisonous_to_pets=true;';
    }
    if (this.growth_rate != '') {
      query += 'growth_rate=' + this.growth_rate + ';';
    }
    if (this.care_level != '') {
      query += 'care_level=' + this.care_level + ';';
    }
    if (this.watering != '') {
      query += 'watering=' + this.watering + ';';
    }
    if (this.sunlight != '') {
      query += 'sunlight=' + this.sunlight + ';';
    }
    console.log(query);

    this.apiConsumer.getPlants(query).subscribe(
      (response: any) => {
        this.dataSource.data = response;
        if (this.dataSource.data.length > 0) {
          this.displayedColumns = Object.keys(this.dataSource.data[0]);
        }
        console.log(response);
      },
      (error) => {
        console.error('There was an error!', error);
      }
    );
  }
}
